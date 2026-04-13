

function downloadFile(fileUrl, fileName) {
    const link = document.createElement('a');
    link.href = fileUrl;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

window.createBlobUrl = function (fileContent) {
    const blob = new Blob([fileContent], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    return URL.createObjectURL(blob);
};