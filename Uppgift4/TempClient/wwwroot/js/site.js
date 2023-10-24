var connection = new signalR.HubConnectionBuilder()
    .withUrl("https://localhost:7045/TemperatureHub")
    .withAutomaticReconnect()
    .build();

connection.start()
    .then(function () {
        console.log("Connected to the server's TemperatureHub");
    })
    .catch(function (err) {
        console.error(err.toString());
    });

connection.on("ReceiveTempertureData", function (forecast) {

    console.log("Date: " + forecast.date);
    console.log("TemperatureC " + forecast.temperatureC);
    console.log("Summary " + forecast.summary);

    var listItem = document.createElement("li");

    listItem.textContent = `Date: ${forecast.date}, Temperature: ${forecast.temperatureC}, Summary: ${forecast.summary}`;

    var forecastList = document.getElementById("forecastList");

    forecastList.appendChild(listItem);
});