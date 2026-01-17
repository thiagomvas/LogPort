import { Line } from 'react-chartjs-2';
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
    TimeScale,
} from 'chart.js';
import 'chartjs-adapter-date-fns';

ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
    TimeScale
);

interface LineGraphProps<T> {
    data: T[];
    xAccessor: (item: T) => string | number | Date;
    yAccessor: (item: T) => number;
    label?: string;
    timeUnit?: 'minute' | 'hour' | 'day';
    height?: number;
}

export const LineGraph = <T,>({
    data,
    xAccessor,
    yAccessor,
    label = 'Values',
    timeUnit = 'hour',
    height = 50,
}: LineGraphProps<T>) => {
    const primaryColor = getComputedStyle(document.documentElement)
        .getPropertyValue('--color-primary')
        .trim();

    const textColor = getComputedStyle(document.documentElement)
        .getPropertyValue('--text-main')
        .trim();

    const chartData = {
        datasets: [
            {
                label,
                data: data.map(item => ({ x: xAccessor(item), y: yAccessor(item) })),
                borderColor: primaryColor,
                backgroundColor: primaryColor + '33',
                tension: 0.3,
                fill: true,
            },
        ],
    };



    const options = {
        responsive: true,
        plugins: {
            legend: {
                display: true,
                labels: { color: textColor },
            },
            tooltip: {
                mode: 'index' as const,
                intersect: false,
            },
        },
        scales: {
            x: {
                type: 'time' as const,
                time: {
                    unit: timeUnit,
                    tooltipFormat: timeUnit === 'day' ? 'MMM d' : 'HH:mm',
                    displayFormats: {
                        minute: 'HH:mm',
                        hour: 'HH:mm',
                        day: 'MMM d',
                    },
                },
                title: {
                    display: true,
                    text: 'Time',
                    color: textColor,
                },
                ticks: {
                    color: textColor,
                },
            },
            y: {
                beginAtZero: true,
                title: {
                    display: true,
                    text: 'Value',
                    color: textColor,
                },
                ticks: {
                    color: textColor,
                },
            },
        },
    };

    return <Line data={chartData} options={options} height={height} />;
};
