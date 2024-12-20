function javascriptConfirm(element, options) {
    const settings = Object.assign({
        title: '',
        message: '',
        confirmText: '',
        cancelText: '',
        onConfirm: function () {},
        onCancel: function () {},
        cancelOnBackdropClick: false,
        width: '500px',
    }, options);
    element.addEventListener('click', function (e) {
        e.preventDefault();
        const backdrop = document.createElement('div');
        backdrop.className = 'confirm-popup-backdrop';
        document.body.appendChild(backdrop);
        
        const confirmBox = document.createElement('div');
        confirmBox.className = 'confirm-popup-box';
        
        if (settings.width !== null && settings.width !== undefined) {
            confirmBox.style.width = settings.width;
        }
        
        const messageBox = document.createElement('div');
        messageBox.className = 'confirm-popup-message-box';
        
        const actionBox = document.createElement('div');
        actionBox.className = 'confirm-popup-actions-box';
        actionBox.style.textAlign = 'right';
        
        if (settings.title) {
            const title = document.createElement('strong');
            title.className = 'confirm-popup-title';
            title.textContent = settings.title;
            messageBox.appendChild(title);
        }
        
        if (settings.message) {
            const message = document.createElement('p');
            message.className = 'confirm-popup-message';
            message.textContent = settings.message;
            messageBox.appendChild(message);
        }
        
        const confirmAction = document.createElement('button');
        confirmAction.className = 'confirm-popup-button confirm-popup-confirm-action';
        confirmAction.textContent = settings.confirmText;
        confirmAction.style.marginRight = '6px';
        applyBtnStyles(confirmAction);
        
        const cancelAction = document.createElement('button');
        cancelAction.className = 'confirm-popup-button confirm-popup-cancel-action';
        cancelAction.textContent = settings.cancelText;
        applyBtnStyles(cancelAction);
        
        actionBox.appendChild(confirmAction);
        actionBox.appendChild(cancelAction);
        
        confirmBox.appendChild(messageBox);
        confirmBox.appendChild(actionBox);
        
        document.body.appendChild(confirmBox);
        
        if (settings.cancelOnBackdropClick) {
            backdrop.addEventListener('click', function () {
                settings.onCancel.call(this);
                document.body.removeChild(confirmBox);
                document.body.removeChild(backdrop);
            }.bind(this));
        }
        
        confirmAction.addEventListener('click', function () {
            settings.onConfirm.call(this);
            document.body.removeChild(confirmBox);
            document.body.removeChild(backdrop);
        }.bind(this));
        
        cancelAction.addEventListener('click', function () {
            settings.onCancel.call(this);
            document.body.removeChild(confirmBox);
            document.body.removeChild(backdrop);
        }.bind(this));
    }.bind(this));
}

function applyBtnStyles(button) {
    const bgColor = '#e6e6e6'
    const bgHoverColor = '#dcdcdc'
    
    button.addEventListener('mouseover', function () {
        this.style.backgroundColor = bgHoverColor;
    });
    
    button.addEventListener('mouseout', function () {
        this.style.backgroundColor = bgColor;
    });
}

