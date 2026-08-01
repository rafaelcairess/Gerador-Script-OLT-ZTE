using GeradorOlt.Nucleo.Modelos;

namespace GeradorOlt.Nucleo.Perfis;

public interface IServicoPerfil
{
    public void Save(string path, DadosPerfil profile);
    public DadosPerfil Load(string path);
}
