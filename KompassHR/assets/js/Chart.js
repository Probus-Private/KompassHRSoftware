
var LeaveChart = document.getElementById('Leave-chart');
window.myDoughnut = new Chart(LeaveChart, {
    type: 'doughnut',
    data: {
        labels: ['Taken', 'Pending', 'Balance'],
        datasets: [{
            data: [2.5, 1, 3.5],
            backgroundColor: ["#8ED1E6", "#E68100", "#4762A5"],

            label: 'CL Leaves'
        }]
    },
    options: {
        plugins:{
            legend: {
                display: true,
                position: 'left',
            }
},
      
        scales: {
            xAxes: {
                gridLines: {
                    drawBorder: false,
                },

                title: {
                    display: true,
                    text: 'CL Leaves'
                },

                ticks: {
                    display: false,
                    // forces step size to be 50 units
                }
            },
            y: {
                display: false,
                title: {
                    display: false,
                    text: 'Value'
                },

                ticks: {
                    display: false,
                }
            }
        }
    }
});
