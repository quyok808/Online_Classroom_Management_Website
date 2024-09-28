// JS có login google ở bài LOGIN GOOGLE thì các bạn vào file comments_v2.js á nha 
// JS có login google ở bài LOGIN GOOGLE thì các bạn vào file comments_v2.js á nha 
const cmtSubmit = document.getElementById("comment_submit");
const cmtArea = document.getElementById("comment_textarea");
const commentWrapper = document.querySelector(".comment_wrapper");
const cmts = [];

cmtSubmit.addEventListener("click", () => {
  console.log(cmtArea.value);
  const curentTime = getCurrentTime();
  const cmt = {
    user: {
      avatar:
        "https://lh3.googleusercontent.com/d/1XmIaSt4u8ZKzPTiPQCR0KNUbZpPfmtV5=s50",
      name: "Turnio DEV",
      mail: "turniodev@gmail.com",
    },
    content: cmtArea.value,
    time: curentTime,
  };
  renderCmt(cmt);
});

function renderCmt(cmt) {
  const newCmt = `
  <div class="comment_item">
  <p>
    <a>
      <img
        src=${cmt.user.avatar}
      />
      <span>${cmt.user.name}</span>
    </a>
    <span>${cmt.time}</span>
  </p>
  <p>
  ${cmt.content}
  </p>
</div>
    `;
  //   commentWrapper.innerHTML += newCmt;
  commentWrapper.insertAdjacentHTML("afterbegin", newCmt);
}

function getCurrentTime() {
  // Tạo một đối tượng Date mới chứa thời gian hiện tại
  var now = new Date();

  // Lấy ngày, tháng, năm, giờ và phút
  var date = now.getDate(); // Lấy ngày (1-31)
  var month = now.getMonth() + 1; // Lấy tháng (0-11), cần cộng thêm 1 vì tháng bắt đầu từ 0
  var year = now.getFullYear(); // Lấy năm (đầy đủ bốn chữ số)
  var hours = now.getHours(); // Lấy giờ (0-23)
  var minutes = now.getMinutes(); // Lấy phút (0-59)

  // Định dạng lại chuỗi theo định dạng mong muốn: dd/mm/yyyy hh:mm
  var formattedDateTime =
    (date < 10 ? "0" : "") +
    date +
    "/" +
    (month < 10 ? "0" : "") +
    month +
    "/" +
    year +
    " " +
    (hours < 10 ? "0" : "") +
    hours +
    ":" +
    (minutes < 10 ? "0" : "") +
    minutes;

  // Hiển thị kết quả
  return formattedDateTime;
}
//auth/userinfo.email 
//auth/userinfo.profile
// const CLIENT_ID = 
//   "945873124961-sg3ja41kspun095seg92ju8ah92qs12l.apps.googleusercontent.com";
// const LINK_ACCESS_TOKEN = `
// https://accounts.google.com/o/oauth2/v2/auth?scope=https%3A//www.googleapis.com/auth/userinfo.email%20https%3A//www.googleapis.com/auth/userinfo.profile&response_type=token&redirect_uri=https%3A//oauth2.example.com/code&client_id=${CLIENT_ID}
// `;

// function redirectLogin(){
//   window.location.href = LINK_ACCESS_TOKEN;
// }

