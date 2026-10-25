using GeradorOlt.Apresentacao.ModelosVisao;
using GeradorOlt.Nucleo.Modelos;
using Xunit;

namespace GeradorOlt.Testes;

public sealed class TestesConfiguracaoViewModel
{
    [Fact]
    public void BuildConfiguration_MapsFormStateToDomainModel()
    {
        var viewModel = new ConfiguracaoViewModel
        {
            Slot = "2",
            Card = "4",
            PonStart = "3",
            PonEnd = "5",
            Vlan = "300",
            OnuStart = "10",
            OnuEnd = "20",
            Prefix = "empresa",
            IsSecondaryServiceEnabled = true,
            SecondaryVlan = "400"
        };

        var configuration = viewModel.BuildConfiguration();

        Assert.Equal(2, configuration.Slot);
        Assert.Equal(4, configuration.Card);
        Assert.Equal(3, configuration.PonStart);
        Assert.Equal(5, configuration.PonEnd);
        Assert.Equal(300, configuration.Vlan);
        Assert.Equal(400, configuration.SecondaryVlan);
        Assert.Equal("empresa", configuration.Prefix);
    }

    [Fact]
    public void BuildConfiguration_RejectsNonNumericInput()
    {
        var viewModel = new ConfiguracaoViewModel
        {
            PonStart = "um"
        };

        var exception = Assert.Throws<FormatException>(
            viewModel.BuildConfiguration);

        Assert.Contains("PON inicial", exception.Message);
    }

    [Fact]
    public void Apply_UpdatesAllFieldsAndRaisesSingleChange()
    {
        var viewModel = new ConfiguracaoViewModel();
        var changeCount = 0;
        viewModel.Changed += (_, _) => changeCount++;

        viewModel.Apply(new DadosPerfil
        {
            Slot = "7",
            Vlan = "700",
            Prefix = "perfil",
            SecondaryService = true,
            SecondaryVlan = "701"
        });

        Assert.Equal(1, changeCount);
        Assert.Equal("7", viewModel.Slot);
        Assert.Equal("700", viewModel.Vlan);
        Assert.Equal("perfil", viewModel.Prefix);
        Assert.True(viewModel.IsSecondaryServiceEnabled);
        Assert.Equal("701", viewModel.SecondaryVlan);
    }

    [Fact]
    public void NumericField_ExposesAndClearsInlineValidationError()
    {
        var viewModel = new ConfiguracaoViewModel
        {
            Vlan = "4095"
        };

        Assert.True(viewModel.HasErrors);
        Assert.Contains("4094", Assert.Single(viewModel.GetErrors(nameof(viewModel.Vlan)).Cast<string>()));

        viewModel.Vlan = "4094";

        Assert.False(viewModel.HasErrors);
        Assert.Empty(viewModel.GetErrors(nameof(viewModel.Vlan)).Cast<string>());
    }

    [Fact]
    public void SecondaryVlan_IsRequiredOnlyWhenSecondaryServiceIsEnabled()
    {
        var viewModel = new ConfiguracaoViewModel();

        viewModel.IsSecondaryServiceEnabled = true;

        Assert.True(viewModel.HasErrors);
        Assert.Single(viewModel.GetErrors(nameof(viewModel.SecondaryVlan)).Cast<string>());

        viewModel.IsSecondaryServiceEnabled = false;

        Assert.False(viewModel.HasErrors);
    }

    [Fact]
    public void GetErrors_ReturnsAllErrorsAndDoesNotRepeatNotificationsForSameMessage()
    {
        var viewModel = new ConfiguracaoViewModel();
        var notifications = 0;
        viewModel.ErrorsChanged += (_, _) => notifications++;

        viewModel.Vlan = "5000";
        viewModel.Vlan = "5001";

        Assert.Single(viewModel.GetErrors(null).Cast<string>());
        Assert.Equal(1, notifications);
    }
}
