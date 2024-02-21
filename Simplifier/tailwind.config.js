/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["**/*.razor", "**/*.cshtml", "**/*.html"],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Helvetica Neue', 'Helvetica', 'Arial', 'sans-serif'],
      },
      colors: {
        'link': '#006bb7',
        'btn-primary': '#1b6ec2',
        'btn-primary-border': '#1861ac',
        'btn-primary-focus-shadow': 'rgba(255, 255, 255, 0.25)',
        'validation-message': '#e50000',
        'valid-outline': '#26b050',
        'invalid-outline': '#e50000',
        'error-boundary': '#b32121',
      },
      boxShadow: {
        'focus': '0 0 0 0.1rem white, 0 0 0 0.25rem #258cfb',
      },
      spacing: {
        'content-pt': '1.1rem',
      }
    },
  },
  plugins: [
    require('@tailwindcss/typography'),
    // Other plugins...
  ],
}
