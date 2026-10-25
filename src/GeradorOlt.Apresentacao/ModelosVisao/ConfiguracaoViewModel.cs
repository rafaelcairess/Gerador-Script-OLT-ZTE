using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using GeradorOlt.Apresentacao.Infraestrutura;
using GeradorOlt.Nucleo.Modelos;
using GeradorOlt.Nucleo.Validacao;

namespace GeradorOlt.Apresentacao.ModelosVisao;

public sealed class ConfiguracaoViewModel : BaseViewModel, INotifyDataErrorInfo
{
    private readonly Dictionary<string, string> _errors = [];
    private bool _suspendNotifications;
    private string _slot = "1";
    private string _card = "3";
    private string _ponStart = "1";
    private string _ponEnd = "1";
    private string _vlan = "100";
    private string _onuStart = "1";
    private string _onuEnd = "128";
    private string _selectedTcontProfile = "1G";
    private string _prefix = "cliente";
    private string _gemName1 = "GEM1";
    private string _gemName2 = "GEM2";
    private string _header = string.Empty;
    private string _footer = string.Empty;
    private bool _isSecondaryServiceEnabled;
    private string _secondaryVlan = string.Empty;

    public event EventHandler? Changed;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public bool HasErrors => _errors.Count > 0;

    [SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "WPF binds this collection through the view-model instance.")]
    public IReadOnlyList<string> TcontProfiles => ValidadorConfiguracao.TcontProfiles;

    public string Slot
    {
        get => _slot;
        set => SetInput(ref _slot, value);
    }

    public string Card
    {
        get => _card;
        set => SetInput(ref _card, value);
    }

    public string PonStart
    {
        get => _ponStart;
        set => SetInput(ref _ponStart, value);
    }

    public string PonEnd
    {
        get => _ponEnd;
        set => SetInput(ref _ponEnd, value);
    }

    public string Vlan
    {
        get => _vlan;
        set => SetInput(ref _vlan, value);
    }

    public string OnuStart
    {
        get => _onuStart;
        set => SetInput(ref _onuStart, value);
    }

    public string OnuEnd
    {
        get => _onuEnd;
        set => SetInput(ref _onuEnd, value);
    }

    public string SelectedTcontProfile
    {
        get => _selectedTcontProfile;
        set => SetInput(ref _selectedTcontProfile, value);
    }

    public string Prefix
    {
        get => _prefix;
        set => SetInput(ref _prefix, value);
    }

    public string GemName1
    {
        get => _gemName1;
        set => SetInput(ref _gemName1, value);
    }

    public string GemName2
    {
        get => _gemName2;
        set => SetInput(ref _gemName2, value);
    }

    public string Header
    {
        get => _header;
        set => SetInput(ref _header, value);
    }

    public string Footer
    {
        get => _footer;
        set => SetInput(ref _footer, value);
    }

    public bool IsSecondaryServiceEnabled
    {
        get => _isSecondaryServiceEnabled;
        set
        {
            if (SetProperty(ref _isSecondaryServiceEnabled, value))
            {
                ValidateNumericField(
                    nameof(SecondaryVlan),
                    SecondaryVlan,
                    1,
                    ValidadorConfiguracao.MaxVlan,
                    required: value);
                NotifyChanged();
            }
        }
    }

    public string SecondaryVlan
    {
        get => _secondaryVlan;
        set => SetInput(ref _secondaryVlan, value);
    }

    public ConfiguracaoOlt BuildConfiguration()
    {
        int? secondaryVlan = IsSecondaryServiceEnabled
            ? ParseInteger(SecondaryVlan, "VLAN do serviço 2")
            : null;

        return new ConfiguracaoOlt(
            Slot: ParseInteger(Slot, "Slot"),
            Card: ParseInteger(Card, "Card"),
            PonStart: ParseInteger(PonStart, "PON inicial"),
            PonEnd: ParseInteger(PonEnd, "PON final"),
            Vlan: ParseInteger(Vlan, "VLAN principal"),
            OnuStart: ParseInteger(OnuStart, "ONU inicial"),
            OnuEnd: ParseInteger(OnuEnd, "ONU final"),
            TcontProfile: SelectedTcontProfile,
            Prefix: Prefix,
            SecondaryVlan: secondaryVlan,
            Header: NormalizeOptionalText(Header),
            Footer: NormalizeOptionalText(Footer),
            GemName1: GemName1,
            GemName2: GemName2);
    }

    public DadosPerfil ToProfile() =>
        new()
        {
            Slot = Slot,
            Card = Card,
            PonStart = PonStart,
            PonEnd = PonEnd,
            Vlan = Vlan,
            OnuStart = OnuStart,
            OnuEnd = OnuEnd,
            TcontProfile = SelectedTcontProfile,
            Prefix = Prefix,
            GemName1 = GemName1,
            GemName2 = GemName2,
            Header = Header,
            Footer = Footer,
            SecondaryService = IsSecondaryServiceEnabled,
            SecondaryVlan = SecondaryVlan
        };

    public void Apply(DadosPerfil profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        _suspendNotifications = true;
        try
        {
            Slot = profile.Slot;
            Card = profile.Card;
            PonStart = profile.PonStart;
            PonEnd = profile.PonEnd;
            Vlan = profile.Vlan;
            OnuStart = profile.OnuStart;
            OnuEnd = profile.OnuEnd;
            SelectedTcontProfile = profile.TcontProfile;
            Prefix = profile.Prefix;
            GemName1 = profile.GemName1;
            GemName2 = profile.GemName2;
            Header = profile.Header;
            Footer = profile.Footer;
            IsSecondaryServiceEnabled = profile.SecondaryService;
            SecondaryVlan = profile.SecondaryVlan;
        }
        finally
        {
            _suspendNotifications = false;
        }

        NotifyChanged();
    }

    public void Reset() => Apply(new DadosPerfil());

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _errors.Values;
        }

        return _errors.TryGetValue(propertyName, out var error)
            ? new[] { error }
            : Array.Empty<string>();
    }

    private void SetInput(
        ref string field,
        string? value,
        [CallerMemberName] string propertyName = "")
    {
        var normalizedValue = value ?? string.Empty;
        if (SetProperty(ref field, normalizedValue, propertyName))
        {
            ValidateInput(propertyName, normalizedValue);
            NotifyChanged();
        }
    }

    private void ValidateInput(string propertyName, string value)
    {
        switch (propertyName)
        {
            case nameof(Slot):
                ValidateNumericField(propertyName, value, 1, ValidadorConfiguracao.MaxSlot);
                break;
            case nameof(Card):
                ValidateNumericField(propertyName, value, 1, ValidadorConfiguracao.MaxCard);
                break;
            case nameof(PonStart):
            case nameof(PonEnd):
                ValidateNumericField(propertyName, value, 1, ValidadorConfiguracao.MaxPon);
                break;
            case nameof(Vlan):
                ValidateNumericField(propertyName, value, 1, ValidadorConfiguracao.MaxVlan);
                break;
            case nameof(OnuStart):
            case nameof(OnuEnd):
                ValidateNumericField(propertyName, value, 1, ValidadorConfiguracao.MaxOnu);
                break;
            case nameof(SecondaryVlan):
                ValidateNumericField(
                    propertyName,
                    value,
                    1,
                    ValidadorConfiguracao.MaxVlan,
                    required: IsSecondaryServiceEnabled);
                break;
        }
    }

    private void ValidateNumericField(
        string propertyName,
        string value,
        int minimum,
        int maximum,
        bool required = true)
    {
        string? error = null;
        if (!required)
        {
            error = null;
        }
        else if (string.IsNullOrWhiteSpace(value))
        {
            error = "Campo obrigatório.";
        }
        else if (!int.TryParse(value.Trim(), out var parsed))
        {
            error = "Informe um número inteiro.";
        }
        else if (parsed < minimum || parsed > maximum)
        {
            error = $"Use um valor entre {minimum} e {maximum}.";
        }

        SetError(propertyName, error);
    }

    private void SetError(string propertyName, string? error)
    {
        bool changed;
        if (error is null)
        {
            changed = _errors.Remove(propertyName);
        }
        else if (!_errors.TryGetValue(propertyName, out var current)
            || !string.Equals(current, error, StringComparison.Ordinal))
        {
            _errors[propertyName] = error;
            changed = true;
        }
        else
        {
            changed = false;
        }

        if (changed)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            OnPropertyChanged(nameof(HasErrors));
        }
    }

    private void NotifyChanged()
    {
        if (!_suspendNotifications)
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    private static int ParseInteger(string value, string field)
    {
        if (!int.TryParse(value.Trim(), out var parsed))
        {
            throw new FormatException($"'{field}' deve ser um número inteiro.");
        }

        return parsed;
    }

    private static string? NormalizeOptionalText(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
