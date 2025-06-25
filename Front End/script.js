// DOM Elements
const sections = {
    login: document.getElementById('login-section'),
    home: document.getElementById('home-section'),
    about: document.getElementById('about-section'),
    profile: document.getElementById('profile-section')
};

const navLinks = document.querySelectorAll('.nav-link');
const searchInput = document.getElementById('search-input');
const searchBtn = document.getElementById('search-btn');
const categoryFilter = document.getElementById('category-filter');
const articlesList = document.getElementById('articles-list');
const addArticleBtn = document.getElementById('add-article-btn');
const addArticleModal = document.getElementById('add-article-modal');
const closeModal = document.querySelector('.close-modal');
const cancelArticleBtn = document.getElementById('cancel-article-btn');
const articleForm = document.getElementById('article-form');
const loginForm = document.getElementById('login-form');
const logoutBtn = document.getElementById('logout-btn');
const backToHomeBtn = document.getElementById('back-to-home-btn');
const usernameDisplay = document.getElementById('username-display');
const toast = document.getElementById('toast');

// Profile elements
const profileUsername = document.getElementById('profile-username');
const profileEmail = document.getElementById('profile-email');
const profileLastLogin = document.getElementById('profile-last-login');
const profileDateJoined = document.getElementById('profile-date-joined');

// State
let currentUser = null;
let articles = [];
let categories = ['All'];

// Mock data - Replace with actual API calls in a real implementation
const mockArticles = [
    {
        id: 1,
        title: 'Getting Started with JavaScript',
        category: 'Programming',
        content: 'JavaScript is a versatile programming language...',
        author: 'muham',
        createdAt: '2025-06-20T10:30:00Z'
    },
    {
        id: 2,
        title: 'CSS Best Practices',
        category: 'Web Design',
        content: 'Cascading Style Sheets (CSS) is a stylesheet language...',
        author: 'muham',
        createdAt: '2025-06-21T14:45:00Z'
    },
    {
        id: 3,
        title: 'Responsive Design Principles',
        category: 'Web Design',
        content: 'Responsive web design is an approach to web design...',
        author: 'muham',
        createdAt: '2025-06-22T09:15:00Z'
    }
];

// Initialize the app
function init() {
    // Check if user is logged in (in a real app, you'd check session/token)
    const loggedInUser = localStorage.getItem('currentUser');
    
    if (loggedInUser) {
        currentUser = JSON.parse(loggedInUser);
        showHomePage();
        loadArticles();
        updateUserDisplay();
    } else {
        showLoginPage();
    }
    
    setupEventListeners();
}

function setupEventListeners() {
    // Navigation
    navLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const sectionId = link.getAttribute('href').substring(1);
            showSection(sectionId);
            
            // Update active nav link
            navLinks.forEach(navLink => navLink.classList.remove('active'));
            link.classList.add('active');
        });
    });
    
    // Search
    searchBtn.addEventListener('click', handleSearch);
    searchInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            handleSearch();
        }
    });
    
    // Category filter
    categoryFilter.addEventListener('change', filterArticles);
    
    // Article modal
    addArticleBtn.addEventListener('click', () => {
        addArticleModal.classList.remove('hidden');
    });
    
    closeModal.addEventListener('click', () => {
        addArticleModal.classList.add('hidden');
    });
    
    cancelArticleBtn.addEventListener('click', () => {
        addArticleModal.classList.add('hidden');
        articleForm.reset();
    });
    
    // Forms
    articleForm.addEventListener('submit', handleArticleSubmit);
    loginForm.addEventListener('submit', handleLogin);
    logoutBtn.addEventListener('click', handleLogout);
    backToHomeBtn.addEventListener('click', () => showSection('home'));
    
    // Close modal when clicking outside
    window.addEventListener('click', (e) => {
        if (e.target === addArticleModal) {
            addArticleModal.classList.add('hidden');
        }
    });
}

// Show specific section
function showSection(sectionId) {
    // Hide all sections first
    Object.values(sections).forEach(section => {
        section.classList.add('hidden');
    });
    
    // Show the requested section
    switch(sectionId) {
        case 'home':
            showHomePage();
            break;
        case 'about':
            sections.about.classList.remove('hidden');
            break;
        case 'profile':
            showProfilePage();
            break;
        case 'login':
            showLoginPage();
            break;
        default:
            showHomePage();
    }
}

function showHomePage() {
    sections.home.classList.remove('hidden');
    loadArticles();
}

function showLoginPage() {
    sections.login.classList.remove('hidden');
    // Hide other sections
    Object.values(sections).forEach(section => {
        if (section !== sections.login) {
            section.classList.add('hidden');
        }
    });
}

function showProfilePage() {
    sections.profile.classList.remove('hidden');
    if (currentUser) {
        profileUsername.textContent = currentUser.username;
        profileEmail.textContent = currentUser.email;
        profileLastLogin.textContent = new Date(currentUser.lastLogin || new Date()).toLocaleString();
        profileDateJoined.textContent = new Date(currentUser.dateJoined || new Date()).toLocaleString();
    }
}

// Article functions
function loadArticles() {
    // In a real app, you would fetch from API
    // Simulating API call with setTimeout
    showLoading();
    
    setTimeout(() => {
        articles = mockArticles;
        renderArticles(articles);
        updateCategories();
        hideLoading();
    }, 500);
}

function renderArticles(articlesToRender) {
    articlesList.innerHTML = '';
    
    if (articlesToRender.length === 0) {
        articlesList.innerHTML = '<p class="no-articles">No articles found.</p>';
        return;
    }
    
    articlesToRender.forEach(article => {
        const articleCard = document.createElement('div');
        articleCard.className = 'article-card';
        
        articleCard.innerHTML = `
            <div class="article-content">
                <h3 class="article-title">${article.title}</h3>
                <p class="article-excerpt">${article.content.substring(0, 100)}...</p>
                <div class="article-meta">
                    <span class="article-category">Category: ${article.category}</span>
                    <a href="#" class="read-more" data-id="${article.id}">Read More</a>
                </div>
            </div>
        `;
        
        articlesList.appendChild(articleCard);
    });
    
    // Add event listeners to "Read More" buttons
    document.querySelectorAll('.read-more').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const articleId = parseInt(btn.getAttribute('data-id'));
            viewArticle(articleId);
        });
    });
}

function viewArticle(articleId) {
    // In a real app, you would fetch the full article from API
    const article = articles.find(a => a.id === articleId);
    if (article) {
        // Create a modal to view the full article
        const modalContent = `
            <div class="modal-content">
                <span class="close-modal">&times;</span>
                <h2>${article.title}</h2>
                <div class="article-meta">
                    <p><strong>Category:</strong> ${article.category}</p>
                    <p><strong>Author:</strong> ${article.author}</p>
                    <p><strong>Date:</strong> ${new Date(article.createdAt).toLocaleDateString()}</p>
                </div>
                <div class="article-full-content">
                    <p>${article.content}</p>
                </div>
            </div>
        `;
        
        const modal = document.createElement('div');
        modal.className = 'modal';
        modal.innerHTML = modalContent;
        document.body.appendChild(modal);
        
        modal.classList.remove('hidden');
        
        // Close modal
        modal.querySelector('.close-modal').addEventListener('click', () => {
            modal.classList.add('hidden');
            setTimeout(() => modal.remove(), 300);
        });
        
        // Close when clicking outside
        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                modal.classList.add('hidden');
                setTimeout(() => modal.remove(), 300);
            }
        });
    }
}

function updateCategories() {
    // Get unique categories from articles
    const uniqueCategories = [...new Set(articles.map(article => article.category))];
    categories = ['All', ...uniqueCategories];
    
    // Update category filter dropdown
    categoryFilter.innerHTML = '';
    categories.forEach(category => {
        const option = document.createElement('option');
        option.value = category;
        option.textContent = category;
        categoryFilter.appendChild(option);
    });
}

function filterArticles() {
    const selectedCategory = categoryFilter.value;
    
    if (selectedCategory === 'All') {
        renderArticles(articles);
    } else {
        const filteredArticles = articles.filter(article => article.category === selectedCategory);
        renderArticles(filteredArticles);
    }
}

function handleSearch() {
    const query = searchInput.value.trim().toLowerCase();
    
    if (query === '') {
        renderArticles(articles);
        return;
    }
    
    const filteredArticles = articles.filter(article => 
        article.title.toLowerCase().includes(query) || 
        article.content.toLowerCase().includes(query)
    );
    
    renderArticles(filteredArticles);
}

// Form handlers
function handleArticleSubmit(e) {
    e.preventDefault();
    
    const title = document.getElementById('article-title').value.trim();
    const category = document.getElementById('article-category').value.trim();
    const content = document.getElementById('article-content').value.trim();
    
    if (!title || !category || !content) {
        showToast('Please fill in all fields', 'error');
        return;
    }
    
    // In a real app, you would send this to the API
    const newArticle = {
        id: articles.length + 1,
        title,
        category,
        content,
        author: currentUser.username,
        createdAt: new Date().toISOString()
    };
    
    // Simulate API call
    showLoading();
    
    setTimeout(() => {
        articles.unshift(newArticle); // Add to beginning of array
        renderArticles(articles);
        updateCategories();
        addArticleModal.classList.add('hidden');
        articleForm.reset();
        showToast('Article added successfully!', 'success');
        hideLoading();
    }, 800);
}

function handleLogin(e) {
    e.preventDefault();
    
    const email = document.getElementById('login-email').value.trim();
    const password = document.getElementById('login-password').value.trim();
    
    if (!email || !password) {
        showToast('Please enter both email and password', 'error');
        return;
    }
    
    // In a real app, you would validate with the API
    // This is a mock implementation
    showLoading();
    
    setTimeout(() => {
        // Mock user data
        currentUser = {
            username: email.split('@')[0],
            email,
            lastLogin: new Date().toISOString(),
            dateJoined: new Date().toISOString()
        };
        
        // In a real app, you would store a token, not the whole user
        localStorage.setItem('currentUser', JSON.stringify(currentUser));
        
        showHomePage();
        updateUserDisplay();
        loginForm.reset();
        showToast('Login successful!', 'success');
        hideLoading();
    }, 1000);
}

function handleLogout() {
    // In a real app, you would also invalidate the token on the server
    localStorage.removeItem('currentUser');
    currentUser = null;
    showLoginPage();
    updateUserDisplay();
    showToast('Logged out successfully', 'success');
}

// UI Helpers
function updateUserDisplay() {
    if (currentUser) {
        usernameDisplay.textContent = currentUser.username;
        // Show user sections, hide login
        sections.login.classList.add('hidden');
    } else {
        usernameDisplay.textContent = '';
    }
}

function showLoading() {
    // In a real app, you might show a spinner
    articlesList.innerHTML = '<div class="loading-spinner"><i class="fas fa-spinner fa-spin"></i> Loading...</div>';
}

function hideLoading() {
    // Loading state would be hidden when content is loaded
}

function showToast(message, type = 'success') {
    toast.textContent = message;
    toast.className = 'toast show ' + type;
    
    setTimeout(() => {
        toast.classList.remove('show');
    }, 3000);
}

// Initialize the app
document.addEventListener('DOMContentLoaded', init);
