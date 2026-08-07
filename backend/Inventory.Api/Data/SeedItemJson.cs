using System.Text.Json.Serialization;

namespace Inventory.Api.Data;

// Mirrors the format of the local seed file (locals/claude/cadastrar.json, user-provided
// test data, gitignored). The JSON property names below match that file's Portuguese keys
// on purpose — it's local dev-only data, not part of the public repository.
public class SeedItemJson
{
    [JsonPropertyName("numero_patrimonio")]
    public string? AssetNumber { get; set; }

    [JsonPropertyName("foto")]
    public required string Photo { get; set; }

    [JsonPropertyName("tipo_do_item")]
    public required string ItemType { get; set; }

    [JsonPropertyName("modelo_e_marca")]
    public required string ModelBrand { get; set; }

    [JsonPropertyName("informacoes_adicionais")]
    public string? AdditionalInfo { get; set; }

    [JsonPropertyName("disponibilidade")]
    public required string Availability { get; set; }

    [JsonPropertyName("estado")]
    public required string Condition { get; set; }
}
