using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AX2012PerformanceOptimizer.Core.Models.PerformanceStack;

namespace AX2012PerformanceOptimizer.WpfApp.Views.Controls;

public partial class LayerCard : UserControl
{
    public static readonly DependencyProperty LayerTypeProperty =
        DependencyProperty.Register(nameof(LayerType), typeof(LayerType), typeof(LayerCard),
            new PropertyMetadata(LayerType.Database, OnLayerTypeChanged));

    public static readonly DependencyProperty LayerMetricsProperty =
        DependencyProperty.Register(nameof(LayerMetrics), typeof(object), typeof(LayerCard),
            new PropertyMetadata(null, OnLayerMetricsChanged));

    public static readonly DependencyProperty IsBottleneckProperty =
        DependencyProperty.Register(nameof(IsBottleneck), typeof(bool), typeof(LayerCard),
            new PropertyMetadata(false, OnBottleneckChanged));

    public LayerType LayerType
    {
        get => (LayerType)GetValue(LayerTypeProperty);
        set => SetValue(LayerTypeProperty, value);
    }

    public object? LayerMetrics
    {
        get => GetValue(LayerMetricsProperty);
        set => SetValue(LayerMetricsProperty, value);
    }

    public bool IsBottleneck
    {
        get => (bool)GetValue(IsBottleneckProperty);
        set => SetValue(IsBottleneckProperty, value);
    }

    public event EventHandler<LayerType>? LayerClicked;

    public LayerCard()
    {
        InitializeComponent();
    }

    private static void OnLayerTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LayerCard card)
        {
            card.UpdateLayerAppearance();
        }
    }

    private static void OnLayerMetricsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LayerCard card)
        {
            card.UpdateMetrics();
        }
    }

    private static void OnBottleneckChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LayerCard card)
        {
            card.UpdateBottleneckHighlighting();
        }
    }

    private void UpdateLayerAppearance()
    {
        switch (LayerType)
        {
            case LayerType.Database:
                MainBorder.Background = new SolidColorBrush(Color.FromRgb(255, 235, 238));
                MainBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(244, 67, 54));
                IconTextBlock.Text = "💾";
                TitleTextBlock.Text = "Database Layer";
                SubtitleTextBlock.Text = "SQL Server database";
                break;
            case LayerType.AosServer:
                MainBorder.Background = new SolidColorBrush(Color.FromRgb(255, 243, 224));
                MainBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 152, 0));
                IconTextBlock.Text = "🖥️";
                TitleTextBlock.Text = "AOS Server Layer";
                SubtitleTextBlock.Text = "Application Object Server";
                break;
            case LayerType.Network:
                MainBorder.Background = new SolidColorBrush(Color.FromRgb(227, 242, 253));
                MainBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(33, 150, 243));
                IconTextBlock.Text = "🌐";
                TitleTextBlock.Text = "Network Layer";
                SubtitleTextBlock.Text = "Network communication";
                break;
            case LayerType.Client:
                MainBorder.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233));
                MainBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                IconTextBlock.Text = "💻";
                TitleTextBlock.Text = "Client Layer";
                SubtitleTextBlock.Text = "End-user client application";
                break;
        }
    }

    private void UpdateMetrics()
    {
        // This would be implemented to populate metrics based on LayerMetrics property
        // For now, metrics are displayed directly in the main view
    }

    private void UpdateBottleneckHighlighting()
    {
        if (IsBottleneck)
        {
            MainBorder.BorderThickness = new Thickness(4);
            MainBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Red,
                BlurRadius = 10,
                ShadowDepth = 0,
                Opacity = 0.5
            };
        }
        else
        {
            MainBorder.BorderThickness = new Thickness(3);
            MainBorder.Effect = null;
        }
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        LayerClicked?.Invoke(this, LayerType);
    }
}
