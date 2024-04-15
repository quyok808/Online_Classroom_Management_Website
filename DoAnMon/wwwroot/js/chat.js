"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you
    // should be aware of possible script injection concerns.
    const currentTime = new Date();
    const year = currentTime.getFullYear(); // Năm (ví dụ: 2024)
    const month = currentTime.getMonth() + 1; // Tháng (từ 0 - 11, cần cộng thêm 1)
    const day = currentTime.getDate(); // Ngày trong tháng
    const hours = currentTime.getHours(); // Giờ (24-giờ)
    const minutes = currentTime.getMinutes(); // Phút
    const seconds = currentTime.getSeconds(); // Giây
    const formatTime = `${hours}:${minutes}:${seconds} - ${day}/${month}/${year}`;
    li.textContent = `${user} (${formatTime}) says ${message}`;
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", user, message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();

    document.getElementById("messageInput").value = "";
    document.getElementById("messageInput").focus;
});

document.getElementById("messageInput").addEventListener("keydown", function (event) {
    if (event.key == 'Enter' && !event.shiftKey) {
        console.log(event.key)
        document.getElementById("sendButton").click();
        event.preventDefault();
    }
});