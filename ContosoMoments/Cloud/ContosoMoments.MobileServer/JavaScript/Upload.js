
window.blobface = {};
blobface.maxBlockSize = 1 * 1024 * 1024; // 1MB per block
var sasurl;
function initializeNewFile(filetmp, url) {
    blobface.numberOfBlocks = 1;
    blobface.selectedFile = filetmp;
    blobface.currentFilePointer = 0;
    blobface.totalBytesRemaining = 0;
    blobface.blockIds = new Array();
    blobface.blockIdPrefix = "block_";
    blobface.submitUri = url;
    blobface.bytesUploaded = 0;
    blobface.inPauseMode = false;
    blobface.cancel = false;
    blobface.retry = 0;
    blobface.uncommittedBlocks = [];
    blobface.threadCount = 6;
    blobface.threads = [];
    blobface.retryCount = 0;
    blobface.error = false;
    blobface.threadFinished = 0;
}

function resumeUpload(file, url) {
    setFile(file, url, true);

}




var startTime, endTime;

function setFile(filetmp, url, isResume, uncommittedBlocks) {

    startTime = new Date();
    sasurl = url;
    url = url + "&comp=block&blockid=";
    
    if (!isResume) {
        initializeNewFile(filetmp, url);
    }
    else {
        blobface.inPauseMode = false;
    }

    var fileSize = blobface.selectedFile.size;
    if (fileSize < blobface.maxBlockSize) {
        maxBlockSize = fileSize;
        console.log("max block size = " + maxBlockSize);
    }

    blobface.totalBytesRemaining = fileSize;
    blobface.numberOfBlocks = Math.ceil(fileSize / blobface.maxBlockSize)

    if (uncommittedBlocks && uncommittedBlocks.length > 0)
        blobface.uncommittedBlocks = uncommittedBlocks;

    console.log("total blocks = " + blobface.numberOfBlocks);

    if (blobface.numberOfBlocks < blobface.threadCount) {
        blobface.threadCount = blobface.numberOfBlocks;
    }


    var filePointer = 0;
    var totalBytesRemainingForThread = Math.ceil(blobface.totalBytesRemaining / blobface.threadCount);
    var bloksForThread = Math.ceil(totalBytesRemainingForThread / blobface.maxBlockSize);

    for (var i = 0; i < blobface.threadCount; i++) {

        var pointer = i * totalBytesRemainingForThread;
        var bytesRemaining = totalBytesRemainingForThread;
        var id = i + 1;
        var startBlockNumber = i * bloksForThread;
        var blocks = blobface.uncommittedBlocks.slice(startBlockNumber, (i + 1) * bloksForThread);

        var th = new Thread(blobface.selectedFile, pointer, bytesRemaining, id, startBlockNumber, blocks);

        if (i == blobface.threadCount - 1) {
            th.totalBytesRemaining = th.totalBytesRemaining + (blobface.totalBytesRemaining % blobface.threadCount);
        }

        blobface.threads.push(th);
    }

    for (var i = 0; i < blobface.threadCount; i++) {
        blobface.threads[i].uploadFileInBlocks();
    }
}


function threadFinish() {
    blobface.threadFinished++;

    if (blobface.threadFinished == blobface.threadCount) {

        for (var i = 0; i < blobface.threadCount; i++) {
            for (var j = 0; j < blobface.threads[i].blockIds.length; j++) {
                blobface.blockIds.push(blobface.threads[i].blockIds[j]);
            }
        }

        allThreadFinish();
    }
}

var containerName;
var fileName;


function allThreadFinish() {
    commit(sasurl);
    sasurl = "";
}



function retry(num) {
    blobface.retryCount += num;

    if (blobface.retryCount % blobface.threadCount == 0) {
        blobface.retry++;
    }
}

function setPercentComplete(upload) {
    blobface.bytesUploaded += upload;
    var percentComplete = ((parseFloat(blobface.bytesUploaded) / parseFloat(blobface.selectedFile.size)) * 100).toFixed(2);
    $.event.trigger('setPercentComplete', [percentComplete]);
    console.log(percentComplete + " %");
}

function hideError() {
    //if (blobface.error) {
    blobface.retry = 0;
    blobface.retryCount = 0;

    blobface.error = false;
    //}
}

function showError() {
    //if (!blobface.error) {
    blobface.error = true;
    //}
}

function cancelUpload() {
    blobface.cancel = true;
}

///////////////////THREAD///////////////////////
var Thread = function (file, currentFilePointer, totalBytesRemaining, id, startBlockNumber, uncommittedBlocks) {
    var self = this;

    self.id = id;
    self.file = file;
    self.currentFilePointer = currentFilePointer;
    self.totalBytesRemaining = totalBytesRemaining;
    self.maxBlockSize = blobface.maxBlockSize;
    self.startBlockNumber = startBlockNumber;
    self.blockIds = new Array();
    self.uncommittedBlocks = uncommittedBlocks;

    self.reader = new FileReader();
    self.reader.onloadend = function (evt) {
        if (evt.target.readyState == FileReader.DONE) {

            var uri = blobface.submitUri + self.blockIds[self.blockIds.length - 1];
            self.log('reader finish with url ' + uri);

            var requestData = new Uint8Array(evt.target.result);
            self.callAjax(requestData, uri);
        }
    };

    self.callAjax = function (requestData, uri) {
        self.log('start Ajax');
        if (blobface.cancel) return;

        retry(1);

        $.ajax({
            url: uri,
            type: "PUT",
            data: requestData,
            processData: false,
            beforeSend: function (xhr) {
                xhr.setRequestHeader('x-ms-blob-type', 'BlockBlob');
            },
            success: function (data, status) {
                self.log(data);
                self.log(status);

                setPercentComplete(requestData.length);
                self.uploadFileInBlocks();

                retry(-1);
                hideError();
            },
            error: function (xhr, desc, err) {
                self.log(desc);
                self.log(err);

                showError();

                setTimeout(function () {
                    self.callAjax(requestData, uri);
                }, 10 * 1000);
            }
        });
    }

    self.uploadFileInBlocks = function () {
        if (blobface.inPauseMode || blobface.cancel) {
            return;
        }

        while (self.uncommittedBlocks[self.blockIds.length]) {
            var blockId = blobface.blockIdPrefix + self.pad(self.blockIds.length + self.startBlockNumber, 6);
            self.log(blockId + ' is alredy in server');

            self.blockIds.push(btoa(blockId));
            self.currentFilePointer += self.maxBlockSize;
            self.totalBytesRemaining -= self.maxBlockSize;
            setPercentComplete(blobface.maxBlockSize);
            if (self.totalBytesRemaining < self.maxBlockSize) {
                self.maxBlockSize = self.totalBytesRemaining;
            }
        }

        if (self.totalBytesRemaining > 0) {
            if (self.totalBytesRemaining < self.maxBlockSize) {
                self.maxBlockSize = self.totalBytesRemaining;
            }

            self.log("current file pointer = " + self.currentFilePointer + " bytes read = " + self.maxBlockSize);
            var fileContent = null;
            if (navigator.userAgent.indexOf('Safari') > -1 && navigator.userAgent.indexOf('Chrome') == -1) {
                //in Safari
                fileContent = self.file.webkitSlice(self.currentFilePointer, self.currentFilePointer + self.maxBlockSize);
            } else {
                //In other browser
                fileContent = self.file.slice(self.currentFilePointer, self.currentFilePointer + self.maxBlockSize);
            }

            var blockId = blobface.blockIdPrefix + self.pad(self.blockIds.length + self.startBlockNumber, 6);
            self.log("block id = " + blockId);
            self.blockIds.push(btoa(blockId));
            self.reader.readAsArrayBuffer(fileContent);
            self.currentFilePointer += self.maxBlockSize;
            self.totalBytesRemaining -= self.maxBlockSize;
        }
        else {
            threadFinish();
        }
    }

    self.pad = function (number, length) {
        var str = '' + number;
        while (str.length < length) {
            str = '0' + str;
        }
        return str;
    }

    self.log = function (msg) {
        console.log("thread id: " + self.id + " - " + msg);
    }
}
