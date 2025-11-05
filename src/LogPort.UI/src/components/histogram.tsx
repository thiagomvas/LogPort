import { Bar } from 'react-chartjs-2';
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    BarElement,
    Title,
    Tooltip,
    Legend,
    TimeScale,
} from 'chart.js';
import type { LogBucket } from '../lib/types/analytics';
import 'chartjs-adapter-date-fns';

ChartJS.register(
    CategoryScale,
    LinearScale,
    BarElement,
    Title,
    Tooltip,
    Legend,
    TimeScale
);


interface HistogramChartProps {
    data: LogBucket[];
}
const primaryColor = getComputedStyle(document.documentElement)
    .getPropertyValue('--color-primary')
    .trim(); // e.g. "#2196f3"
const textColor = getComputedStyle(document.documentElement)
    .getPropertyValue('--text-main')
    .trim()

export const HistogramChart = ({ data }: HistogramChartProps) => {
    const chartData = {
        labels: data.map(bucket => new Date(bucket.periodStart)),
        datasets: [
            {
                label: 'Log Count',
                data: data.map(bucket => bucket.count),
                backgroundColor: primaryColor,
                borderColor: primaryColor,
                borderWidth: 1,
                color: textColor
            },
        ],
    };

    const options = {
        responsive: true,
        plugins: {
            legend: { display: true, labels: { color: textColor } },
            tooltip: { mode: 'index' as const },
        },
        scales: {
            x: {
                type: 'time' as const,
                time: {
                    unit: 'minute' as 'minute',
                    tooltipFormat: 'HH:mm',
                    displayFormats: { hour: 'HH:mm' },
                },
                title: { display: true, text: 'Time', color: textColor },
                ticks: { stepSize: 1, color: textColor },
            },
            y: {
                beginAtZero: true,
                title: { display: true, text: 'Count', color: textColor },
                ticks: { color: textColor },
            },
        },

    };

    return <Bar data={chartData} options={options} height={100} />;
};
