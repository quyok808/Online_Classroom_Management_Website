document.addEventListener("DOMContentLoaded", function () {
    // Lấy địa chỉ URL hiện tại
    var url = window.location.href;

    // Lấy tất cả các mục trong thanh menu
    var menuItems = document.querySelectorAll('nav ul li a');

    // Lặp qua từng mục trong thanh menu
    menuItems.forEach(function (item) {
        // Kiểm tra nếu địa chỉ URL chứa đường dẫn của mục đó
        if (url.indexOf(item.getAttribute('href')) > -1) {
            // Thêm lớp 'selected' cho mục đó
            item.classList.add('selected');
        }
    });
});

