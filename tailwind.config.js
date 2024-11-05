/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        // Include all Blazor components
        "./**/*.{razor,html,cshtml}",
        // Specifically target the most common locations
        "./Pages/**/*.{razor,html,cshtml}",
        "./Shared/**/*.{razor,html,cshtml}",
        "./Components/**/*.{razor,html,cshtml}",
        // Include any additional HTML files
        "./wwwroot/**/*.html",
        "./wwwroot/index.html"
    ],
    theme: {
        extend: {},
    },
    plugins: [],
}