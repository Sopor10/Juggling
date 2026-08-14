using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Siteswaps.Generator.Components.CardStackPage.Models;
using Siteswaps.Generator.Core.Generator;

namespace Siteswaps.Generator.Components.CardStackPage;

public partial class CardStackPage : ComponentBase, IAsyncDisposable
{
    private const int MinSpinnerVisibleMs = 500;

    private static readonly List<int> JugglerOptions = [2, 3, 4, 5, 6, 7, 8];

    private readonly CardStackFormState _state = new();
    private readonly List<Siteswap> _results = [];

    private int _nextFilterId = 1;
    private bool _sheetOpen;
    private CardStackFilterItem? _editingItem;

    private bool _isGenerating;
    private bool _hasGenerated;
    private CancellationTokenSource? _cts;

    private ElementReference _pageRef;
    private ElementReference _footerRef;
    private IJSObjectReference? _jsModule;
    private IJSObjectReference? _footerObserver;
    private DotNetObjectReference<CardStackPage>? _selfReference;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized()
    {
        Navigation.LocationChanged += OnLocationChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>(
            "import",
            "./_content/Siteswaps.Generator/Components/CardStackPage/CardStackPage.razor.js"
        );
        _footerObserver = await _jsModule.InvokeAsync<IJSObjectReference>(
            "initFooterObserver",
            _footerRef,
            _pageRef
        );
        _selfReference = DotNetObjectReference.Create(this);
        await _jsModule.InvokeVoidAsync("initHistory", _selfReference);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (!IsCardStackLocation(e.Location))
        {
            _cts?.Cancel();
            _isGenerating = false;
        }
    }

    private static bool IsCardStackLocation(string location) =>
        location.Contains("/cardstack", StringComparison.OrdinalIgnoreCase);

    [JSInvokable]
    public async Task OnBrowserPopState(bool showResults)
    {
        _cts?.Cancel();
        _isGenerating = false;
        _hasGenerated = showResults && _results.Count > 0;
        await InvokeAsync(StateHasChanged);
    }

    private void OnJugglersExactChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var parsed))
        {
            _state.Jugglers = Math.Clamp(
                parsed,
                CardStackFormState.MinJugglers,
                CardStackFormState.MaxJugglersExact
            );
        }
    }

    private void OpenAddFilterSheet()
    {
        _editingItem = null;
        _sheetOpen = true;
    }

    private void OpenEditFilterSheet(CardStackFilterItem item)
    {
        _editingItem = item;
        _sheetOpen = true;
    }

    private void SaveFilter(CardStackFilterItem draft)
    {
        if (draft.Id == 0)
        {
            var assigned = draft.Clone();
            var withId = new CardStackFilterItem
            {
                Id = _nextFilterId++,
                Kind = assigned.Kind,
                NumberComparison = assigned.NumberComparison,
                NumberAmount = assigned.NumberAmount,
                NumberThrowHeight = assigned.NumberThrowHeight,
                PatternRotation = assigned.PatternRotation,
                PatternIsInclude = assigned.PatternIsInclude,
                PatternSequenceHeights = assigned.PatternSequenceHeights,
                StateActiveBeats = assigned.StateActiveBeats,
            };

            if (_state.Filters.Count > 0)
            {
                _state.Connectors.Add(CardStackFilterConnector.And);
            }

            _state.Filters.Add(withId);
        }
        else
        {
            var index = _state.Filters.FindIndex(f => f.Id == draft.Id);
            if (index >= 0)
            {
                _state.Filters[index] = draft;
            }
        }
    }

    private void DeleteFilter(int id)
    {
        var index = _state.Filters.FindIndex(f => f.Id == id);
        if (index == -1)
        {
            return;
        }

        _state.Filters.RemoveAt(index);
        if (_state.Connectors.Count > 0)
        {
            var connectorIndex = Math.Min(index, _state.Connectors.Count - 1);
            _state.Connectors.RemoveAt(connectorIndex);
        }
    }

    private async Task GenerateAsync()
    {
        if (_isGenerating || _sheetOpen)
        {
            return;
        }

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _isGenerating = true;
        _hasGenerated = false;
        _results.Clear();
        StateHasChanged();
        await Task.Yield();

        var startedAt = Environment.TickCount64;
        var generators = FilterTranslation.CreateSiteswapGenerators(_state);
        var sinceLastRender = 0;

        try
        {
            foreach (var generator in generators)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                await foreach (var siteswap in generator.GenerateAsync(token))
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    _results.Add(siteswap);
                    sinceLastRender++;
                    if (sinceLastRender >= 10)
                    {
                        sinceLastRender = 0;
                        StateHasChanged();
                        await Task.Delay(1);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation from a superseded run.
        }

        var elapsed = Environment.TickCount64 - startedAt;
        if (elapsed < MinSpinnerVisibleMs)
        {
            await Task.Delay((int)(MinSpinnerVisibleMs - elapsed));
        }

        if (!token.IsCancellationRequested)
        {
            _isGenerating = false;
            _hasGenerated = true;
            StateHasChanged();

            if (_results.Count > 0 && _jsModule is not null)
            {
                await _jsModule.InvokeVoidAsync("pushResultsState");
                await _jsModule.InvokeVoidAsync("scrollIntoView", ".cs-results-section");
            }
        }
        else
        {
            _isGenerating = false;
            StateHasChanged();
        }
    }

    public async ValueTask DisposeAsync()
    {
        Navigation.LocationChanged -= OnLocationChanged;
        _cts?.Cancel();

        try
        {
            if (_footerObserver is not null)
            {
                await _footerObserver.InvokeVoidAsync("dispose");
                await _footerObserver.DisposeAsync();
            }

            if (_jsModule is not null)
            {
                if (_selfReference is not null)
                {
                    await _jsModule.InvokeVoidAsync("disposeHistory", _selfReference);
                }

                await _jsModule.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // Circuit already gone.
        }

        _selfReference?.Dispose();
    }
}
