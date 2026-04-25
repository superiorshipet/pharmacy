using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using DawaeeBackend.Data;
using DawaeeBackend.Models;

namespace DawaeeBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatbotController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChatbotController> _logger;
    
    private static string? _cachedMedsContent = null;
    private static DateTime _cacheTime = DateTime.MinValue;
    private const int CACHE_MINUTES = 30;

    public ChatbotController(
        ApplicationDbContext context,
        IConfiguration config,
        ILogger<ChatbotController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _config = config;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    private async Task<string> GetCachedMedicationsContent()
    {
        if (_cachedMedsContent != null && DateTime.UtcNow.Subtract(_cacheTime).TotalMinutes < CACHE_MINUTES)
            return _cachedMedsContent;

        try
        {
            var medications = await _context.Medications.ToListAsync();
            if (!medications.Any())
            {
                _cachedMedsContent = "لا توجد أدوية في قاعدة البيانات حالياً.";
                _cacheTime = DateTime.UtcNow;
                return _cachedMedsContent;
            }

            var sb = new StringBuilder();
            sb.AppendLine("قائمة الأدوية المتوفرة:\n");
            
            foreach (var med in medications.Take(50))
            {
                var descAr = med.DescriptionAr ?? "";
                var warningsAr = med.WarningsAr ?? "";
                var shortDesc = descAr.Length > 100 ? descAr.Substring(0, 100) + "..." : descAr;
                var shortWarn = warningsAr.Length > 80 ? warningsAr.Substring(0, 80) + "..." : warningsAr;
                
                sb.AppendLine($"• {med.NameAr} - {med.ActiveIngredientAr}");
                sb.AppendLine($"  الاستخدام: {shortDesc}");
                sb.AppendLine($"  التحذير: {shortWarn}\n");
            }

            _cachedMedsContent = sb.ToString();
            _cacheTime = DateTime.UtcNow;
            return _cachedMedsContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building medications content");
            return "معلومات الأدوية غير متاحة حالياً.";
        }
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
    {
        if (request.Messages == null || request.Messages.Count == 0)
            return BadRequest(new { error = "الرسالة فارغة" });

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
            return Unauthorized();

        var apiKey = _config["Groq:ApiKey"];
        var model = _config["Groq:Model"] ?? "mixtral-8x7b-32768";
        
        try
        {
            var medsContent = await GetCachedMedicationsContent();
            var diseasesText = string.IsNullOrEmpty(user.ChronicDiseases) ? "لا يوجد" : user.ChronicDiseases;
            
            var systemPrompt = $@"أنت مساعد طبي ذكي اسمك ""دواءي"". أنت ودود ومتخصص.

معلومات المريض: الأمراض المزمنة - {diseasesText}

قاعدة بيانات الأدوية:
{medsContent}

تعليمات مهمة:
1. رد بشكل طبيعي ومتنوع - لا تكرر نفس الرد أبداً
2. استخدم لغة مختلفة في كل مرة
3. جاوب على السؤال مباشرة وبإيجاز (جملتين إلى 3 جمل كحد أقصى)
4. استخدم الرموز التعبيرية المناسبة
5. إذا سأل عن صحتك أو عمرك، أجب بشكل طبيعي ومختلف
6. لا تقدم تشخيصات طبية";

            string reply;
            
            if (!string.IsNullOrEmpty(apiKey) && apiKey != "your-free-key-here")
            {
                reply = await GetGroqResponse(systemPrompt, request.Messages, model, apiKey);
                if (string.IsNullOrEmpty(reply))
                {
                    reply = GetDynamicLocalResponse(request.Messages.Last().Message, user);
                }
            }
            else
            {
                reply = GetDynamicLocalResponse(request.Messages.Last().Message, user);
            }

            // Save chat history
            var chatMessage = new Models.ChatMessage
            {
                UserId = userId,
                Message = request.Messages.Last().Message,
                Response = reply,
                CreatedAt = DateTime.UtcNow
            };
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            return Ok(new { reply });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat error");
            var fallbackReply = GetDynamicLocalResponse(request.Messages.Last().Message, user);
            return Ok(new { reply = fallbackReply });
        }
    }

    private async Task<string> GetGroqResponse(string systemPrompt, List<ChatMessageDto> messages, string model, string apiKey)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            
            var allMessages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            var history = messages.TakeLast(8).ToList();
            foreach (var msg in history)
                allMessages.Add(new { role = msg.Role, content = msg.Message });

            var groqRequest = new
            {
                model = model,
                messages = allMessages,
                max_tokens = 200,
                temperature = 0.9,
                top_p = 0.95
            };

            var json = JsonSerializer.Serialize(groqRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var chatResponse = JsonSerializer.Deserialize<GroqResponse>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var reply = chatResponse?.Choices?[0]?.Message?.Content;
                
                if (!string.IsNullOrEmpty(reply))
                    return reply;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Groq API exception");
            return null;
        }
    }

    private string GetDynamicLocalResponse(string userMessage, User user)
    {
        userMessage = userMessage.ToLower();
        
        var responses = new List<string>();
        
        // Emergency
        if (userMessage.Contains("طوارئ") || userMessage.Contains("اسعاف") || userMessage.Contains("نزيف") || userMessage.Contains("قلب"))
        {
            return "🚨 حالة طارئة! يرجى التوجه فوراً لأقرب مستشفى أو الاتصال بالإسعاف 997. لا تتردد!";
        }
        
        // Greetings - متنوعة
        if (userMessage.Contains("مرحب") || userMessage.Contains("السلام") || userMessage.Contains("هلا"))
        {
            responses.Add("وعليكم السلام! 👋 أنا دواءي، تحت أمرك. استفسارك الطبي؟");
            responses.Add("أهلاً بك! كيف أقدر أخدمك اليوم في المجال الطبي؟ 😊");
            responses.Add("الله يحييك! أنا دواءي، جاهز لمساعدتك الطبية.");
            return responses[new Random().Next(responses.Count)];
        }
        
        // How are you - متنوعة
        if (userMessage.Contains("كيفك") || userMessage.Contains("اخبارك"))
        {
            responses.Add("بخير وسعادة، شكراً لسؤالك! وأنت كيف صحتك اليوم؟ 😊");
            responses.Add("تمام التمام، الحمد لله! أخبرني كيف أقدر أساعدك؟");
            responses.Add("أنا بخير، متحمس لمساعدتك! أخبرني عن استفسارك الطبي.");
            return responses[new Random().Next(responses.Count)];
        }
        
        // Age - متنوعة
        if (userMessage.Contains("عمر") || userMessage.Contains("سنه"))
        {
            responses.Add("عمري الرقمي بضعة أشهر، لكن معلوماتي طبية محدثة باستمرار! 😊");
            responses.Add("أنا حديث العهد بالعالم الرقمي، عمري لا يتجاوز الشهور. لكن معرفتي واسعة!");
            responses.Add("بالنسبة للعمر، أنا مساعد ذكي جديد! عمري الرقمي صغير جداً.");
            return responses[new Random().Next(responses.Count)];
        }
        
        // Thank you
        if (userMessage.Contains("شكر") || userMessage.Contains("ممتاز"))
        {
            responses.Add("العفو! هذا واجبي. هل هناك شيء آخر أساعدك فيه؟");
            responses.Add("على الرحب والسعة! دايماً موجود لخدمتك.");
            responses.Add("بكل سرور! تفضل بأي استفسار طبي آخر.");
            return responses[new Random().Next(responses.Count)];
        }
        
        // Medication
        var medications = _context.Medications.ToListAsync().GetAwaiter().GetResult();
        var foundMed = medications.FirstOrDefault(m => 
            userMessage.Contains(m.NameAr.ToLower()) || 
            userMessage.Contains(m.NameEn.ToLower()));
            
        if (foundMed != null)
        {
            return $"📋 {foundMed.NameAr}\n🔬 المادة الفعالة: {foundMed.ActiveIngredientAr}\n💊 الاستخدام: {foundMed.DescriptionAr}\n⚠️ تحذير: {foundMed.WarningsAr}";
        }
        
        // Default - متنوعة
        responses.Add("هل تريد معلومات عن دواء معين؟ اكتب اسم الدواء وسأعطيك كل التفاصيل.");
        responses.Add("أنا هنا لمساعدتك! يمكنك سؤالي عن أي دواء أو استفسار طبي.");
        responses.Add("كيف أقدر أساعدك؟ اسأل عن دواء معين أو نصائح طبية عامة.");
        return responses[new Random().Next(responses.Count)];
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetChatHistory()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var history = await _context.ChatMessages
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.CreatedAt)
            .Take(50)
            .ToListAsync();

        return Ok(history.Select(c => new
        {
            c.Id,
            c.Message,
            c.Response,
            c.CreatedAt
        }));
    }
}

public class ChatRequestDto
{
    public List<ChatMessageDto> Messages { get; set; } = new();
}

public class ChatMessageDto
{
    public string Role { get; set; } = "user";
    public string Message { get; set; } = string.Empty;
}

public class GroqResponse
{
    public GroqChoice[] Choices { get; set; } = Array.Empty<GroqChoice>();
}

public class GroqChoice
{
    public GroqMessage Message { get; set; } = new();
}

public class GroqMessage
{
    public string Content { get; set; } = string.Empty;
}
