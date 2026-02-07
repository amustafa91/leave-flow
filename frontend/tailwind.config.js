/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        "./src/**/*.{html,ts}",
    ],
    theme: {
        extend: {
            colors: {
                primary: {
                    DEFAULT: '#059669',
                    dark: '#047857',
                },
                secondary: '#0891b2',
                accent: '#f59e0b',
            }
        },
    },
    plugins: [],
}
