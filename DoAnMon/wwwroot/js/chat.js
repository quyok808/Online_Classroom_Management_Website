"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    var classId = document.getElementById("classId").value;

    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });

    event.preventDefault();
});

document.getElementById("messageInput").addEventListener("keydown", function (event) {
    if (event.key == 'Enter' && !event.shiftKey) {
        var user = document.getElementById("userInput").value;
        var message = document.getElementById("messageInput").value;
        var classId = document.getElementById("classId").value;

        connection.invoke("SendMessage", user, message).catch(function (err) {
            return console.error(err.toString());
        });

        event.preventDefault();
        event.preventDefault();
    }
});