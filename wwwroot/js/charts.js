// charts.js - Backup chart functions (glavne su u _Host.cshtml)

// Legacy support funkcije
window.initializeChart = function (canvasId, config) {
    return window.ChartJsInterop.updateChart(canvasId, config);
};

window.updateChart = function (canvasId, config) {
    return window.ChartJsInterop.updateChart(canvasId, config);
};

window.destroyChart = function (canvasId) {
    return window.ChartJsInterop.destroyChart(canvasId);
};

// Dodatne helper funkcije za debugging
window.chartDebug = {
    listCharts: function() {
        console.log('Active charts:', Object.keys(window.ChartJsInterop.chartInstances));
        return Object.keys(window.ChartJsInterop.chartInstances);
    },
    
    getChart: function(canvasId) {
        return window.ChartJsInterop.chartInstances[canvasId];
    },
    
    destroyAllCharts: function() {
        Object.keys(window.ChartJsInterop.chartInstances).forEach(canvasId => {
            window.ChartJsInterop.destroyChart(canvasId);
        });
        console.log('All charts destroyed');
    }
};

console.log('Charts.js loaded - legacy support enabled');
