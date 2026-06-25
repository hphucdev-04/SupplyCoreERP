import pg from 'pg';
import dotenv from 'dotenv';
import path from 'path';
import { fileURLToPath } from 'url';
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const nodeEnv = process.env.NODE_ENV || 'development';
// Load cấu hình biến môi trường tương ứng (.env cho local dev và .env.production cho production)
dotenv.config({
    path: path.resolve(__dirname, nodeEnv === 'production' ? '../.env.production' : '../.env')
});
const { Pool } = pg;
// Khởi tạo connection pool từ connection string trong .env
const pool = new Pool({
    connectionString: process.env.DATABASE_URL,
    max: parseInt(process.env.DB_POOL_MAX || '10'),
    idleTimeoutMillis: parseInt(process.env.DB_IDLE_TIMEOUT || '30000'), // 30 giây
    connectionTimeoutMillis: parseInt(process.env.DB_CONNECTION_TIMEOUT || '5000'), // 5 giây, 
});
/**
 * Thực thi câu lệnh SQL truy vấn Database PostgreSQL
 * @param text Câu lệnh SQL
 * @param params Các tham số truyền vào
 * @returns Mảng các dòng kết quả
 */
export const queryDb = async (text, params) => {
    const start = Date.now();
    try {
        const res = await pool.query(text, params);
        const duration = Date.now() - start;
        console.error('[Database] Executed query:', {
            durationMs: duration,
            rowsCount: res.rowCount
        });
        return res.rows;
    }
    catch (error) {
        console.error('[Database] Query error:', error);
        throw error;
    }
};
