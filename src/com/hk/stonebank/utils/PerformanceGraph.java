package com.hk.stonebank.utils;

import com.hk.stonebank.settings.Settings;
import org.jfree.chart.ChartFactory;
import org.jfree.chart.ChartUtils;
import org.jfree.data.xy.XYSeries;
import org.jfree.data.xy.XYSeriesCollection;

import java.awt.*;
import java.io.IOException;
import java.util.HashMap;

public class PerformanceGraph {

    public PerformanceGraph(HashMap<Integer, Long> data) throws IOException {

        var xy_series = new XYSeries("Performance");

        for (int iteration : data.keySet())
            xy_series.add(iteration, data.get(iteration));

        var xy_dataset = new XYSeriesCollection();
        xy_dataset.addSeries(xy_series);

        var chart = ChartFactory.createXYLineChart(
                "Performance (screenshot delay excluded: " + Settings.SCREENSHOT_DELAY + " ms)",
                "Iteration",
                "Time (ms)",
                xy_dataset
        );

        chart.getPlot().setBackgroundPaint(Color.WHITE);

        ChartUtils.saveChartAsPNG(Settings.PERFORMANCE_GRAPH_OUTPUT, chart, 600, 400);

    }

}
