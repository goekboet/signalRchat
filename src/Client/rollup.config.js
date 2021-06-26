import resolve from '@rollup/plugin-node-resolve';
import { terser } from "rollup-plugin-terser";

export default {
  input: 'src/app.js',
  context: 'window',
  output: {
    file: '../Server/wwwroot/app.js',
    format: 'es'
  },
  plugins: [ resolve({jsnext: true }), terser() ]
};