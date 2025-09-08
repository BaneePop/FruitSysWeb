window.downloadFile = (data, filename, mimeType) => {
    const blob = new Blob([Uint8Array.from(atob(data), c => c.charCodeAt(0))], {
        type: mimeType
    });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
};