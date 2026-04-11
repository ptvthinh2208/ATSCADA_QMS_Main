function renderChart(chartCanvasId, labels, data, title, chartColor) {
    var ctx = document.getElementById(chartCanvasId).getContext('2d');

    // Đặt lại kích thước của canvas nếu cần
    var canvas = document.getElementById(chartCanvasId);
    canvas.width = canvas.offsetWidth;  // Sử dụng chiều rộng của phần tử
    canvas.height = 300;  // Hoặc chiều cao phù hợp với thiết kế của bạn

    var chartData = {
        labels: labels,
        datasets: [{
            label: title,
            data: data,
            backgroundColor: chartColor,
            borderColor: chartColor.replace('0.2', '1'),
            borderWidth: 1
        }]
    };

    new Chart(ctx, {
        type: 'line',
        data: chartData,
        options: {
            responsive: true,
            maintainAspectRatio: false,
        }
    });
}

function updateChart(data) {
    // Xóa và tạo lại phần tử canvas
    const container = document.getElementById('chartContainer');
    container.innerHTML = '<canvas id="myChart"></canvas>';
    const ctx = document.getElementById('myChart').getContext('2d');


    // Trích xuất dữ liệu labels và values từ `data`
    var labels = Object.keys(data); // Ngày
    var values = Object.values(data); // Số liệu
    
    var myChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels, // Trục x
            datasets: [{
                label: 'Số lượng',
                data: values, // Trục y
                borderColor: 'rgba(29, 162, 180, 1)',
                backgroundColor: 'rgba(29, 162, 180, 0.2)',
                fill: false
            }]
        },
        options: {
            scales: {
                x: {
                    type: 'category',
                    title: { display: true, text: 'Ngày' }
                },
                y: {
                    beginAtZero: true,
                    title: { display: true, text: 'Số lượng' }
                }
            }
        }
    });
}