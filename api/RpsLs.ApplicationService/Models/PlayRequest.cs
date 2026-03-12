using System.ComponentModel.DataAnnotations;

namespace RpsLs.ApplicationService.Models;

public record PlayRequest([Range(1, 5)] int Player);
