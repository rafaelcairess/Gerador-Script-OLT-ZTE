using GeradorOlt.Nucleo.Modelos;

namespace GeradorOlt.Nucleo.Perfis;

public interface IServicoPerfil
{
    void Save(string path, DadosPerfil profile);
    DadosPerfil Load(string path);
}
