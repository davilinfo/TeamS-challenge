using System.ComponentModel.DataAnnotations;

namespace RpsLs.Api.Models;

public record PlayRequest([Range(1, 5)] int Player);
