class Chatbot {
    constructor() {
        this.isOpen = false;
        this.widget = document.getElementById('chatbotWidget');
        this.toggleBtn = document.getElementById('chatbotToggle');
        this.closeBtn = document.getElementById('chatbotClose');
        this.messagesContainer = document.getElementById('chatbotMessages');
        this.input = document.getElementById('chatbotInput');
        this.sendBtn = document.getElementById('chatbotSend');
        this.suggestionsContainer = document.getElementById('chatbotSuggestions');

        this.init();
    }

    init() {
        this.toggleBtn.addEventListener('click', () => this.toggle());
        this.closeBtn.addEventListener('click', () => this.close());
        this.sendBtn.addEventListener('click', () => this.sendMessage());
        this.input.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') this.sendMessage();
        });

        // Load initial suggestions
        this.loadSuggestions();

        // Add suggestion button handlers
        this.suggestionsContainer.addEventListener('click', (e) => {
            if (e.target.classList.contains('suggestion-btn')) {
                const message = e.target.getAttribute('data-message');
                this.input.value = message;
                this.sendMessage();
            }
        });
    }

    toggle() {
        this.isOpen = !this.isOpen;
        this.widget.style.display = this.isOpen ? 'flex' : 'none';
        this.toggleBtn.style.display = this.isOpen ? 'none' : 'block';
        
        if (this.isOpen) {
            this.input.focus();
        }
    }

    close() {
        this.isOpen = false;
        this.widget.style.display = 'none';
        this.toggleBtn.style.display = 'block';
    }

    async sendMessage() {
        const message = this.input.value.trim();
        if (!message) return;

        // Add user message
        this.addMessage(message, true);
        this.input.value = '';

        // Show typing indicator
        this.showTypingIndicator();

        try {
            const response = await fetch('/api/chatbot/send', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ message: message, isUser: true })
            });

            const data = await response.json();
            
            // Remove typing indicator
            this.removeTypingIndicator();
            
            // Add bot response
            this.addMessage(data.response, false);
            
            // Update suggestions
            this.updateSuggestions(data.suggestions);

        } catch (error) {
            console.error('Chatbot error:', error);
            this.removeTypingIndicator();
            this.addMessage("I'm having trouble connecting right now. Please try again later.", false);
        }
    }

    addMessage(text, isUser) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `chatbot-message ${isUser ? 'user-message' : 'bot-message'}`;
        
        const time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        
        messageDiv.innerHTML = `
            <div class="message-content">${this.escapeHtml(text)}</div>
            <div class="message-time">${time}</div>
        `;

        this.messagesContainer.appendChild(messageDiv);
        this.messagesContainer.scrollTop = this.messagesContainer.scrollHeight;
    }

    showTypingIndicator() {
        const typingDiv = document.createElement('div');
        typingDiv.className = 'chatbot-message bot-message';
        typingDiv.id = 'typingIndicator';
        typingDiv.innerHTML = `
            <div class="message-content">
                <div class="typing-dots">
                    <span></span>
                    <span></span>
                    <span></span>
                </div>
            </div>
        `;
        this.messagesContainer.appendChild(typingDiv);
        this.messagesContainer.scrollTop = this.messagesContainer.scrollHeight;
    }

    removeTypingIndicator() {
        const typingIndicator = document.getElementById('typingIndicator');
        if (typingIndicator) {
            typingIndicator.remove();
        }
    }

    async loadSuggestions() {
        try {
            const response = await fetch('/api/chatbot/suggestions');
            const suggestions = await response.json();
            this.updateSuggestions(suggestions);
        } catch (error) {
            console.error('Failed to load suggestions:', error);
        }
    }

    updateSuggestions(suggestions) {
        this.suggestionsContainer.innerHTML = '';
        suggestions.forEach(suggestion => {
            const button = document.createElement('button');
            button.className = 'suggestion-btn';
            button.textContent = suggestion;
            button.setAttribute('data-message', suggestion);
            this.suggestionsContainer.appendChild(button);
        });
    }

    escapeHtml(unsafe) {
        return unsafe
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }
}

// Initialize chatbot when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    new Chatbot();
});