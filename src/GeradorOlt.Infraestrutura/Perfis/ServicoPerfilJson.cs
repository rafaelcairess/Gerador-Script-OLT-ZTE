using System.Text;
using System.Text.Json;
using GeradorOlt.Infraestrutura.Arquivos;
using GeradorOlt.Nucleo.Modelos;
using GeradorOlt.Nucleo.Perfis;

namespace GeradorOlt.Infraestrutura.Perfis;

public sealed class ServicoPerfilJson : IServicoPerfil
{
    private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public void Save(string path, DadosPerfil profile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(profile);
        var directory = Path.GetDirectoryName(Path.GetFullPath(path))
            ?? throw new IOException("Não foi possível determinar a pasta do perfil.");
        Directory.CreateDirectory(directory);
        var document = DocumentoPerfil.FromModel(profile);
        EscritorArquivoAtomico.WriteAllText(
            path,
            JsonSerializer.Serialize(document, Options),
            Utf8WithoutBom);
    }

    public DadosPerfil Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try
        {
            var json = File.ReadAllText(path);
            var document = JsonSerializer.Deserialize<DocumentoPerfil>(json, Options)
                ?? throw new InvalidDataException("O arquivo de perfil está vazio ou inválido.");
            if (document.Version < 1 || document.Version > DocumentoPerfil.CurrentVersion)
            {
                throw new InvalidDataException(
                    $"Versão de perfil não suportada: {document.Version}.");
            }

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
