// /auth/userinfo.email
// .../auth/userinfo.profile
const CLIENT_ID =
  "________________________________"; // Thay ID của các bạn vô đây nhe
const LINK_ACCESS_TOKEN = `
https://accounts.google.com/o/oauth2/v2/auth?scope=https%3A//www.googleapis.com/auth/userinfo.email%20https%3A//www.googleapis.com/auth/userinfo.profile&response_type=token&redirect_uri=http://127.0.0.1:5500/single_post.html&client_id=${CLIENT_ID}
`;
// _____________________________-
const cmtSubmit = document.getElementById("comment_submit");
const cmtArea = document.getElementById("comment_textarea");
const commentWrapper = document.querySelector(".comment_wrapper");
const cmts = [];
let user = JSON.parse(localStorage.getItem("user"));

cmtSubmit.addEventListener("click", () => {
  if (user) {
    const curentTime = getCurrentTime();
    const cmt = {
      user: user,
      content: cmtArea.value,
      time: curentTime,
    };
    renderCmt(cmt);
    cmtArea.value = "";
  } else {
    redirectLogin();
  }
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

function redirectLogin() {
  window.location.href = LINK_ACCESS_TOKEN;
}

function getAccessToken() {
  const saveAccessToken = localStorage.getItem("accessToken");
  if (saveAccessToken) {
    getUserInfo(saveAccessToken);
  } else {
    const URL = new URLSearchParams(window.location.hash.substring(1));
    const accessToken = URL.get("access_token");
    if (!accessToken) {
      return;
    }
    localStorage.setItem("accessToken", accessToken);
    getUserInfo(accessToken);
  }
}
getAccessToken();

async function getUserInfo(accessToken) {
  if (!accessToken) {
    return;
  }
  const response = await fetch(
    `https://www.googleapis.com/oauth2/v3/userinfo?access_token=${accessToken}`
  );
  const data = await response.json();
  const { name, picture, email } = data;
  const userLogin = {
    avatar: picture,
    name: name,
    mail: email,
  };
  localStorage.setItem("user", JSON.stringify(userLogin));
   return (user = userLogin);
}
