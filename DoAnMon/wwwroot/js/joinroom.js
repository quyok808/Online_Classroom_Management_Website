
function joinRoom(event) {
    console.log('Clicked!');
    event.preventDefault();
    var roomid = prompt("Nhập mã phòng:");
    if (roomid !== null && roomid.trim() !== "") {
        saveUserIdToDatabase(roomid);
    }
}

function saveUserIdToDatabase(roomid) {
    $.ajax({
        type: "POST",
        url: "/ClassRooms/joinClass",
        data: {
            roomid: roomid
        },
        success: function () {

            alert("Bạn " + @user?.Name + " đã tham gia vào phòng học có mã" + roomid);
        },
        error: function (error) {
            console.error(error);
        }
    });
}
