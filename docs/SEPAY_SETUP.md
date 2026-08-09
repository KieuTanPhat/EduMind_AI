# SePay cho EduMind AI

Luồng thanh toán:

1. Người dùng chọn Plus hoặc Pro.
2. Backend tạo một `PlusRequest` ở trạng thái `Pending` và sinh mã chuyển khoản dạng `EDU...`.
3. Frontend chỉ hiển thị QR sau khi nhận được mã từ backend.
4. SePay gửi webhook khi có tiền vào.
5. Backend kiểm tra xác thực webhook, loại giao dịch, mã chuyển khoản và số tiền. Nếu đúng, gói được kích hoạt 30 ngày.

## Cấu hình local

Tạo các biến môi trường User trên Windows, không đưa giá trị thật vào Git:

```powershell
[Environment]::SetEnvironmentVariable('SEPAY_WEBHOOK_API_KEY', 'copy-api-key-here', 'User')
# Production nên dùng HMAC thay API key:
[Environment]::SetEnvironmentVariable('SEPAY_WEBHOOK_SECRET', 'copy-hmac-secret-here', 'User')
[Environment]::SetEnvironmentVariable('SEPAY_BANK_ACCOUNT_NUMBER', 'YOUR_BANK_ACCOUNT_NUMBER', 'User')
```

Trong `frontend/.env` (tạo từ `.env.example`) điền mã ngân hàng và số tài khoản nhận tiền:

```env
VITE_SEPAY_BANK_CODE=MB
VITE_SEPAY_ACCOUNT_NUMBER=YOUR_BANK_ACCOUNT_NUMBER
```

Khởi động lại terminal/API sau khi đổi biến môi trường. Webhook local cần URL public HTTPS, ví dụ dùng ngrok:

```powershell
ngrok http 5194
```

Đăng nhập SePay → Tích hợp → Webhooks và tạo webhook:

- URL: `https://YOUR_PUBLIC_HOST/api/payments/sepay/webhook`
- Sự kiện: Tiền vào
- Content-Type: JSON
- Cấu trúc mã thanh toán: tiền tố `EDU`, hậu tố 10 chữ số. Backend sinh mã dạng `EDU1234567890` để khớp cấu hình SePay.
- Test dùng API Key; production dùng HMAC-SHA256

Sau khi chuyển sang HMAC, SePay gửi `X-SePay-Timestamp` và `X-SePay-Signature`. Backend kiểm tra timestamp trong 5 phút và ký trên raw body trước khi xử lý.

## Kiểm thử

Trong SePay dùng chức năng gửi thử webhook hoặc Test mode. Payload mẫu cần có `transferType: "in"`, `transferAmount`, và `code` khớp mã `EDU...` của một yêu cầu đang Pending. Chỉ giao dịch đúng số tiền mới được tự động cấp gói; webhook gửi lại sẽ không cấp trùng.

Endpoint đã thêm:

```text
POST /api/payments/sepay/webhook
```

Tài liệu chính thức: https://developer.sepay.vn/vi/sepay-webhooks/tao-webhook
