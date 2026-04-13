function renderReCaptcha(siteKey, elementId) {
    setTimeout(() => {
        const element = document.getElementById(elementId);
        if (element) {
            grecaptcha.render(elementId, {
                'sitekey': siteKey
            });
        } else {
            console.error("reCAPTCHA placeholder element with id '" + elementId + "' not found.");
        }
    }, 2000); // Trì hoãn 500ms
}

