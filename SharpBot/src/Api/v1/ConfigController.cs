using SharpBot;
using SharpBot.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SharpBot.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly ILogger<ConfigController> _logger;
    private IOptionsMonitor<BaiduConfig> source;

    public ConfigController(ILogger<ConfigController> logger,IOptionsMonitor<BaiduConfig> source)
    {
        _logger = logger;
        this.source = source;
    }
    
    [HttpGet("source")]
    public BaiduConfig GetSourceConfig()
    {
        return source.CurrentValue;
    }
}