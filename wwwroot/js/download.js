// wwwroot/js/download.js - ISPRAVKA za PDF download probleme

window.downloadFile = (base64Data, fileName, mimeType) => {
    try {
        console.log('Download initiated:', { fileName, mimeType, dataLength: base64Data.length });

        // ISPRAVKA: Proper base64 decoding
        const binaryString = window.atob(base64Data);
        const bytes = new Uint8Array(binaryString.length);
        
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }

        // ISPRAVKA: Create blob with proper type
        const blob = new Blob([bytes], { type: mimeType });
        
        // VALIDACIJA: Check blob size
        if (blob.size === 0) {
            throw new Error('Generated file is empty');
        }

        console.log('Blob created:', { size: blob.size, type: blob.type });

        // ISPRAVKA: Better download method
        if (window.navigator && window.navigator.msSaveOrOpenBlob) {
            // Internet Explorer
            window.navigator.msSaveOrOpenBlob(blob, fileName);
        } else {
            // Modern browsers
            const url = window.URL.createObjectURL(blob);
            
            const link = document.createElement('a');
            link.href = url;
            link.download = fileName;
            link.style.display = 'none';
            
            document.body.appendChild(link);
            link.click();
            
            // Cleanup
            document.body.removeChild(link);
            
            // ISPRAVKA: Delayed URL cleanup to ensure download completes
            setTimeout(() => {
                window.URL.revokeObjectURL(url);
            }, 1000);
        }

        console.log('Download completed successfully');
        
        // DODATO: Success notification
        showNotification('Fajl je uspešno preuzet!', 'success');

    } catch (error) {
        console.error('Download error:', error);
        alert('Greška pri preuzimanju fajla: ' + error.message);
        
        // DODATO: Error notification  
        showNotification('Greška pri preuzimanju fajla: ' + error.message, 'error');
    }
};

// DODATO: Function to test PDF opening capability
window.testPdfSupport = () => {
    try {
        // Create a minimal valid PDF
        const minimalPdf = '%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj 2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj 3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 612 792]/Resources<<>>>>endobj xref\n0 4\n0000000000 65535 f \n0000000009 00000 n \n0000000058 00000 n \n0000000115 00000 n \ntrailer<</Size 4/Root 1 0 R>>\nstartxref\n198\n%%EOF';
        
        const blob = new Blob([minimalPdf], { type: 'application/pdf' });
        const url = window.URL.createObjectURL(blob);
        
        // Try to open in new tab
        const testWindow = window.open(url, '_blank');
        
        setTimeout(() => {
            if (testWindow) {
                testWindow.close();
            }
            window.URL.revokeObjectURL(url);
        }, 2000);
        
        return true;
    } catch (error) {
        console.error('PDF support test failed:', error);
        return false;
    }
};

// DODATO: Function to show notifications
window.showNotification = (message, type = 'info') => {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'info'} alert-dismissible fade show position-fixed`;
    notification.style.cssText = `
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    `;
    
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
};

// DODATO: Enhanced download with progress for large files
window.downloadFileWithProgress = async (base64Data, fileName, mimeType) => {
    try {
        console.log('Enhanced download initiated:', { fileName, mimeType });
        
        // Show progress for large files
        if (base64Data.length > 1000000) { // > 1MB
            showNotification('Priprema velikog fajla za preuzimanje...', 'info');
        }
        
        // Convert base64 to blob in chunks for large files
        const chunkSize = 1024 * 1024; // 1MB chunks
        const chunks = [];
        
        for (let i = 0; i < base64Data.length; i += chunkSize) {
            const chunk = base64Data.substring(i, i + chunkSize);
            const binaryString = window.atob(chunk);
            const bytes = new Uint8Array(binaryString.length);
            
            for (let j = 0; j < binaryString.length; j++) {
                bytes[j] = binaryString.charCodeAt(j);
            }
            
            chunks.push(bytes);
            
            // Allow UI to update for large files
            if (chunks.length % 10 === 0) {
                await new Promise(resolve => setTimeout(resolve, 1));
            }
        }
        
        const blob = new Blob(chunks, { type: mimeType });
        
        if (blob.size === 0) {
            throw new Error('Generated file is empty');
        }
        
        // Download the blob
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.click();
        
        // Cleanup
        setTimeout(() => {
            window.URL.revokeObjectURL(url);
        }, 1000);
        
        showNotification('Fajl je uspešno preuzet!', 'success');
        
    } catch (error) {
        console.error('Enhanced download error:', error);
        showNotification('Greška pri preuzimanju fajla: ' + error.message, 'error');
    }
};

// DODATO: Function to debug download issues
window.debugDownload = (base64Data, fileName, mimeType) => {
    console.group('Download Debug Info');
    console.log('File name:', fileName);
    console.log('MIME type:', mimeType);
    console.log('Base64 data length:', base64Data.length);
    console.log('First 100 chars of base64:', base64Data.substring(0, 100));
    
    try {
        const binaryString = window.atob(base64Data.substring(0, 100));
        console.log('First 20 bytes as hex:', 
            Array.from(binaryString)
                .slice(0, 20)
                .map(char => char.charCodeAt(0).toString(16).padStart(2, '0'))
                .join(' ')
        );
    } catch (e) {
        console.error('Base64 decode error:', e);
    }
    
    console.groupEnd();
};

// DODATO: Alternative download method if primary fails
window.downloadFileAlternative = (base64Data, fileName, mimeType) => {
    try {
        // Method 1: Data URL approach
        const dataUrl = `data:${mimeType};base64,${base64Data}`;
        const link = document.createElement('a');
        link.href = dataUrl;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        
        showNotification('Fajl preuzet alternativnom metodom', 'success');
    } catch (error) {
        console.error('Alternative download failed:', error);
        
        // Method 2: Open in new window as fallback
        try {
            const dataUrl = `data:${mimeType};base64,${base64Data}`;
            window.open(dataUrl, '_blank');
            showNotification('Fajl otvoren u novom tabu', 'info');
        } catch (e) {
            console.error('All download methods failed:', e);
            showNotification('Sve metode preuzimanja su neuspešne', 'error');
        }
    }
};