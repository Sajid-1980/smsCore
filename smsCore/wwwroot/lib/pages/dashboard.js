
$(function () {
    //student chart
    var campIds = JSON.parse(campIdsServer);
    $.get('/Home/DashboardData', function (response) {
        $('#std-total').html(response.lblStudentTotal);
        $('#std-present').html(response.lblStdPresent);
        $('#std-leave').html(response.lblStdLeave);
        $('#std-absent').html(response.lblStdAbsent);
        $('#emp-total').html(response.lblEmployeeTotal);
        $('#emp-present').html(response.lblEmployeePresent);
        $('#emp-leave').html(response.lblEmployeeLeave);
        $('#emp-absent').html(response.lblEmployeeAbsent);
        $('#free-total').html(response.freeAdmission);
        $('#class-total').html(response.classes);
        $('#prn-total').html(response.lblPrentsTotal);
    })

    $.get('/Home/BirthDays', function (response) {
        if (response.length == 0) {
            var tm = $($('#emptyTemplate').html());
            tm.addClass('active');
            $('#carousel-inner-brithdays').append(tm);
            return false;
        }

        response.forEach(function (v, i, a) {
            var template = $($('#carouselTemplate').html());
            if (i == 0) {
                template.addClass('active');
            }
            template.find('#pbxPhoto').attr('src', v.Photo);
            template.find('#stuName').html(v.StudentName);
            template.find('#lblcampus').html(v.Admission[0].CampusName)
            template.find('#lblclass').html(v.Admission[0].ClassName);
            template.find('#lbldob').html(v.DOB);
            $('#carousel-inner-brithdays').append(template);

        })

    })


    $.ajax({
        type: "GET",
        url: "/api/dashboard/student-chart-data?campusIds=" + campIds,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (chData) {
            var aData = chData;
            var aLabels = aData[0];
            var aDatasets1 = aData[1];
            var dataT = {
                labels: aLabels,
                datasets: aDatasets1

            };
            var ctx = $("#studentChart").get(0).getContext("2d");
            var myNewChart = new Chart(ctx, {
                type: 'bar',
                data: dataT,
                options: {
                    responsive: true,
                    title: { display: true, text: 'Class wise Student Count' },
                    legend: { position: 'bottom' },
                    scales: {
                        xAxes: [{ gridLines: { display: false }, display: true, scaleLabel: { display: false, labelString: '' } }],
                        yAxes: [{ gridLines: { display: false }, display: true, scaleLabel: { display: false, labelString: '' }, ticks: { stepSize: 50, beginAtZero: true } }]
                    },
                }
            });
        }
    });


    // session chart
    $.ajax({
        type: "GET",
        url: "/api/dashboard/session-chart-data",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (chData) {
            var aData = chData;
            var aInfo = aData[0];
            var aLabels = aData[1];
            var aDatasets1 = aData[2];
            var dataT = {
                labels: aLabels,
                datasets: [{
                    label: "# of days",
                    data: aDatasets1,
                    fill: false,
                    backgroundColor: ["rgba(54, 162, 235, 0.2)", "rgba(255, 99, 132, 0.2)", "rgba(255, 159, 64, 0.2)", "rgba(255, 205, 86, 0.2)", "rgba(75, 192, 192, 0.2)", "rgba(153, 102, 255, 0.2)", "rgba(201, 203, 207, 0.2)"],
                    borderColor: ["rgb(54, 162, 235)", "rgb(255, 99, 132)", "rgb(255, 159, 64)", "rgb(255, 205, 86)", "rgb(75, 192, 192)", "rgb(153, 102, 255)", "rgb(201, 203, 207)"],
                    borderWidth: 1
                }]
            };
            var ctx = $("#sessionChart").get(0).getContext("2d");
            var myNewChart = new Chart(ctx, {
                type: 'bar',
                data: dataT,
                options: {
                    responsive: true,
                    title: { display: true, text: 'Current Session Information ' + aInfo.lblSessionInfId + ' (' + aInfo.lblSessionStartDate + ' To ' + aInfo.lblSessionEndDate + ')' },
                    legend: { position: 'bottom', display: false },
                    scales: {
                        xAxes: [{ gridLines: { display: false }, display: true, scaleLabel: { display: false, labelString: '' } }],
                        yAxes: [{ gridLines: { display: false }, display: true, scaleLabel: { display: false, labelString: '' }, ticks: { stepSize: 50, beginAtZero: true } }]
                    },
                }
            });
        }
    });
});