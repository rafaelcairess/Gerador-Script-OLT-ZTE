using System.Text.Json;
using GeradorOlt.Nucleo.Modelos;
using GeradorOlt.Nucleo.Perfis;

namespace GeradorOlt.Infraestrutura.Perfis;

public sealed class ServicoPerfilJson : IServicoPerfil
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public void Save(string path, DadosPerfil profile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(profile);
        var document = DocumentoPerfil.FromModel(profile);
        File.WriteAllText(path, JsonSerializer.Serialize(document, Options));
    }

    public DadosPerfil Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try
        {
            var json = File.ReadAllText(path);
            var document = JsonSerializer.Deserialize<DocumentoPerfil>(json, Options)
                ?? throw new InvalidDataException("O arquivo de perfil está vazio ou inválido.");
            return document.ToModel();
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                "O arquivo de perfil está corrompido ou possui formato inválido.",
                exception);
        }
    }
}
