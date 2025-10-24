-- Thêm cột SwapType vào bảng DoctorShiftExchange
ALTER TABLE DoctorShiftExchange 
ADD SwapType NVARCHAR(20) DEFAULT 'Temporary';

-- Cập nhật dữ liệu hiện tại
UPDATE DoctorShiftExchange 
SET SwapType = 'Temporary' 
WHERE SwapType IS NULL;

