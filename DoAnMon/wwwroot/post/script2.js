const h1 = document.querySelector("h1");
const submitBtn = document.getElementById("submit");
const hide_content = document.getElementById("hide_content");

submitBtn.addEventListener("click", () => {
    const data = editor.getData();
    hide_content.innerHTML = data;
    const imgs = hide_content.querySelectorAll("img");
    const postData = [];

    imgs.forEach((img, index) => {
        const startIndex = img.src.indexOf(":");
        const endIndex = img.src.indexOf(";");
        const type = img.src.slice(startIndex + 1, endIndex);
        postData[index] = {
            name: `${h1.innerText}${index}`,
            type: type,
            data: img.src.split(",")[1],
        };
    });

    postFile(postData);
});

async function postFile(postData) {
    try {
        const response = await fetch(
            "https://script.google.com/macros/s/AKfycbyf_E0bAmFqn9i1m_efPwvYXPfUYMBJc9KQ_J1jKOwlBwFBFIGxLQ7kpEuK3lQJml7r/exec",
            {
                method: "POST",
                body: JSON.stringify(postData),
            }
        );

        if (!response.ok) {
            throw new Error('Network response was not ok');
        }

        const data = await response.json();
        const imgs = hide_content.querySelectorAll("img");

        imgs.forEach((img, index) => {
            img.src = data[index].link;
        });

        const content = hide_content.innerHTML;
        await postToSheets(content);
        redirectToHomePage();
    } catch (error) {
        console.error('Error:', error);
        alert("Vui lòng thử lại");
    }
}

async function postToSheets(content) {
    const formData = new FormData();
    formData.append("entry.1338304216", h1.innerText);
    formData.append("entry.632147546", content);

    try {
        const response = await fetch(
            "https://docs.google.com/forms/d/e/1FAIpQLSfAUIRE8Mm7OHeNaAgcu9XxaPjpDZQSNMlKcEtyj_xDcD-HsQ/formResponse?hl=vi",
            {
                method: "POST",
                body: formData,
                mode: "no-cors",
            }
        );

        if (!response.ok) {
            throw new Error('Failed to post to Google Sheets');
        }

        console.log('Successfully posted to Google Sheets');
    } catch (error) {
        console.error('Error posting to Google Sheets:', error);
    }
}

function redirectToHomePage() {
    window.location.href = '/';
}