namespace AppFactura.Controls;

public sealed class ZoomableImage : ContentView
{
    public static readonly BindableProperty SourceProperty = BindableProperty.Create(
        nameof(Source),
        typeof(ImageSource),
        typeof(ZoomableImage),
        propertyChanged: (bindable, _, value) =>
            ((ZoomableImage)bindable)._image.Source = (ImageSource?)value);

    private readonly Image _image;
    private double _startScale = 1;
    private double _xOffset;
    private double _yOffset;

    public ZoomableImage()
    {
        _image = new Image
        {
            Aspect = Aspect.AspectFit,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        var pinch = new PinchGestureRecognizer();
        pinch.PinchUpdated += OnPinchUpdated;
        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(pinch);
        GestureRecognizers.Add(pan);
        Content = _image;
    }

    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    private void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
    {
        switch (e.Status)
        {
            case GestureStatus.Started:
                _startScale = Content.Scale;
                Content.AnchorX = 0;
                Content.AnchorY = 0;
                break;
            case GestureStatus.Running:
                Content.Scale = Math.Clamp(_startScale * e.Scale, 1, 4);
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (Content.Scale <= 1)
                    ResetPosition();
                break;
        }
    }

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (Content.Scale <= 1)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Running:
                Content.TranslationX = _xOffset + e.TotalX;
                Content.TranslationY = _yOffset + e.TotalY;
                break;
            case GestureStatus.Completed:
                _xOffset = Content.TranslationX;
                _yOffset = Content.TranslationY;
                break;
        }
    }

    private void ResetPosition()
    {
        Content.Scale = 1;
        Content.TranslationX = 0;
        Content.TranslationY = 0;
        _xOffset = 0;
        _yOffset = 0;
    }
}
