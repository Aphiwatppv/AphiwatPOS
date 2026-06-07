(() => {
  const shell = document.querySelector('.app-shell');
  const sidebarToggle = document.querySelector('[data-sidebar-toggle]');
  const collapsedKey = 'aphiwatpos.sidebar.collapsed';
  const languageKey = 'aphiwatpos.language';
  const fallbackLanguage = 'en';
  const originalText = new WeakMap();
  const originalAttributes = new WeakMap();
  let isApplyingLanguage = false;
  const translations = {
    en: {
      'common.cancel': 'Cancel',
      'layout.brandSubtitle': 'Retail security console',
      'layout.defaultSubtitle': 'Secure POS operations and employee access control',
      'logout.confirmBody': 'Are you sure you want to log out of AphiwatPOS?',
      'logout.confirmTitle': 'Confirm Logout',
      'nav.brand': 'Brand',
      'nav.category': 'Category',
      'nav.creditRepayment': 'Credit Repayment',
      'nav.currentStock': 'Current Stock',
      'nav.customer': 'Customer',
      'nav.customerCredit': 'Customer Credit',
      'nav.customerHistory': 'Customer History',
      'nav.customerList': 'Customer List',
      'nav.customerReport': 'Customer Report',
      'nav.dailyClosing': 'Daily Closing',
      'nav.dashboard': 'Dashboard',
      'nav.employees': 'Employees',
      'nav.heldSales': 'Held Sales',
      'nav.inventory': 'Inventory',
      'nav.inventoryDashboard': 'Inventory Dashboard',
      'nav.locations': 'Locations',
      'nav.lowStockProducts': 'Low Stock Products',
      'nav.loyaltyPoints': 'Loyalty Points',
      'nav.memberLevel': 'Member Level',
      'nav.permissions': 'Permissions',
      'nav.product': 'Product',
      'nav.products': 'Products',
      'nav.retailPos': 'Retail POS',
      'nav.retailPosTitle': 'Retail POS Checkout',
      'nav.returnsRefunds': 'Returns / Refunds',
      'nav.roles': 'Roles',
      'nav.sales': 'Sales',
      'nav.salesHistory': 'Sales History',
      'nav.settings': 'Settings',
      'nav.stockAdjustment': 'Stock Adjustment',
      'nav.stockCount': 'Stock Count',
      'nav.stockMovement': 'Stock Movement',
      'nav.stockTransfer': 'Stock Transfer',
      'nav.unit': 'Unit',
      'nav.wholesalePos': 'Wholesale POS',
      'nav.wholesalePosTitle': 'Wholesale Checkout',
      'profile.changePassword': 'Change Password',
      'profile.logout': 'Logout',
      'profile.myProfile': 'My Profile',
      'settings.languageDescription': 'Select the display language for menus and shared controls on this device.',
      'settings.languageNote': 'Your choice is saved in this browser and applies automatically when you return.',
      'settings.languageTitle': 'Language',
      'settings.subtitle': 'Choose how AphiwatPOS looks and speaks for this browser.',
      'settings.title': 'Settings'
    },
    th: {
      'common.cancel': 'ยกเลิก',
      'layout.brandSubtitle': 'ระบบจัดการร้านค้า',
      'layout.defaultSubtitle': 'จัดการงาน POS และสิทธิ์พนักงานอย่างปลอดภัย',
      'logout.confirmBody': 'คุณต้องการออกจากระบบ AphiwatPOS ใช่หรือไม่?',
      'logout.confirmTitle': 'ยืนยันการออกจากระบบ',
      'nav.brand': 'แบรนด์',
      'nav.category': 'หมวดหมู่',
      'nav.creditRepayment': 'ชำระเครดิต',
      'nav.currentStock': 'สต็อกปัจจุบัน',
      'nav.customer': 'ลูกค้า',
      'nav.customerCredit': 'เครดิตลูกค้า',
      'nav.customerHistory': 'ประวัติลูกค้า',
      'nav.customerList': 'รายชื่อลูกค้า',
      'nav.customerReport': 'รายงานลูกค้า',
      'nav.dailyClosing': 'ปิดยอดรายวัน',
      'nav.dashboard': 'แดชบอร์ด',
      'nav.employees': 'พนักงาน',
      'nav.heldSales': 'บิลพักไว้',
      'nav.inventory': 'คลังสินค้า',
      'nav.inventoryDashboard': 'แดชบอร์ดคลังสินค้า',
      'nav.locations': 'ที่ตั้งสินค้า',
      'nav.lowStockProducts': 'สินค้าใกล้หมด',
      'nav.loyaltyPoints': 'คะแนนสะสม',
      'nav.memberLevel': 'ระดับสมาชิก',
      'nav.permissions': 'สิทธิ์การใช้งาน',
      'nav.product': 'สินค้า',
      'nav.products': 'สินค้า',
      'nav.retailPos': 'ขายปลีก POS',
      'nav.retailPosTitle': 'หน้าขายปลีก POS',
      'nav.returnsRefunds': 'คืนสินค้า / คืนเงิน',
      'nav.roles': 'บทบาท',
      'nav.sales': 'การขาย',
      'nav.salesHistory': 'ประวัติการขาย',
      'nav.settings': 'ตั้งค่า',
      'nav.stockAdjustment': 'ปรับสต็อก',
      'nav.stockCount': 'ตรวจนับสต็อก',
      'nav.stockMovement': 'ความเคลื่อนไหวสต็อก',
      'nav.stockTransfer': 'โอนย้ายสต็อก',
      'nav.unit': 'หน่วย',
      'nav.wholesalePos': 'ขายส่ง POS',
      'nav.wholesalePosTitle': 'หน้าขายส่ง',
      'profile.changePassword': 'เปลี่ยนรหัสผ่าน',
      'profile.logout': 'ออกจากระบบ',
      'profile.myProfile': 'โปรไฟล์ของฉัน',
      'settings.languageDescription': 'เลือกภาษาที่ใช้แสดงเมนูและปุ่มส่วนกลางบนอุปกรณ์นี้',
      'settings.languageNote': 'ระบบจะบันทึกภาษาที่เลือกไว้ในเบราว์เซอร์นี้ และใช้อัตโนมัติเมื่อกลับมาอีกครั้ง',
      'settings.languageTitle': 'ภาษา',
      'settings.subtitle': 'เลือกวิธีแสดงผลและภาษาของ AphiwatPOS สำหรับเบราว์เซอร์นี้',
      'settings.title': 'ตั้งค่า'
    }
  };
  const phraseTranslations = {
    th: {
      'Access Denied': 'ไม่มีสิทธิ์เข้าถึง',
      'Action': 'การทำงาน',
      'Actions': 'การทำงาน',
      'Active': 'ใช้งาน',
      'Add': 'เพิ่ม',
      'Add Customer': 'เพิ่มลูกค้า',
      'Add Note': 'เพิ่มบันทึก',
      'Adjust': 'ปรับ',
      'Alerts / Warnings': 'การแจ้งเตือน / คำเตือน',
      'Allow Credit': 'อนุญาตเครดิต',
      'Amount': 'จำนวนเงิน',
      'Apply': 'นำไปใช้',
      'Available Credit': 'เครดิตคงเหลือ',
      'Available Points': 'คะแนนคงเหลือ',
      'Average bill': 'บิลเฉลี่ย',
      'Balance': 'ยอดคงเหลือ',
      'Bank Transfer': 'โอนธนาคาร',
      'Barcode': 'บาร์โค้ด',
      'Best-Selling Products': 'สินค้าขายดี',
      'Blocked': 'ถูกบล็อก',
      'COGS': 'ต้นทุนขาย',
      'Cancel': 'ยกเลิก',
      'Cash': 'เงินสด',
      'Cash Diff.': 'ส่วนต่างเงินสด',
      'Cash received': 'เงินสดที่รับ',
      'Cashier': 'แคชเชียร์',
      'Cashier Dashboard': 'แดชบอร์ดแคชเชียร์',
      'Cashier Performance': 'ประสิทธิภาพแคชเชียร์',
      'Cashier Report': 'รายงานแคชเชียร์',
      'Category': 'หมวดหมู่',
      'Change Amount': 'เงินทอน',
      'Change Password': 'เปลี่ยนรหัสผ่าน',
      'Clear': 'ล้าง',
      'Code': 'รหัส',
      'Code, name, phone': 'รหัส ชื่อ หรือเบอร์โทร',
      'Complaint': 'ข้อร้องเรียน',
      'Confirm Logout': 'ยืนยันการออกจากระบบ',
      'Continue Held Sale': 'ทำบิลพักไว้ต่อ',
      'Cost': 'ต้นทุน',
      'Credit': 'เครดิต',
      'Credit History': 'ประวัติเครดิต',
      'Credit Limit': 'วงเงินเครดิต',
      'Credit Reminder': 'แจ้งเตือนเครดิต',
      'Credit Repayment': 'ชำระเครดิต',
      'Credit Status': 'สถานะเครดิต',
      'Credit Term Days': 'ระยะเครดิต (วัน)',
      'Created': 'วันที่สร้าง',
      'Created By': 'สร้างโดย',
      'Current': 'ปัจจุบัน',
      'Current Cart': 'ตะกร้าปัจจุบัน',
      'Current Stock': 'สต็อกปัจจุบัน',
      'Customer': 'ลูกค้า',
      'Customer Credit': 'เครดิตลูกค้า',
      'Customer Display': 'จอลูกค้า',
      'Customer History': 'ประวัติลูกค้า',
      'Customer List': 'รายชื่อลูกค้า',
      'Customer Overview': 'ภาพรวมลูกค้า',
      'Customer Report': 'รายงานลูกค้า',
      'Customer code may be auto-generated.': 'ระบบอาจสร้างรหัสลูกค้าให้อัตโนมัติ',
      'Daily Closing': 'ปิดยอดรายวัน',
      'Dashboard': 'แดชบอร์ด',
      'Date From': 'วันที่เริ่มต้น',
      'Date To': 'วันที่สิ้นสุด',
      'Development Mode': 'โหมดพัฒนา',
      'Discount': 'ส่วนลด',
      'Discounts': 'ส่วนลด',
      'Edit': 'แก้ไข',
      'Edit Customer': 'แก้ไขลูกค้า',
      'Edit Note': 'แก้ไขบันทึก',
      'Edit note': 'แก้ไขบันทึก',
      'Edit user': 'แก้ไขผู้ใช้',
      'Email': 'อีเมล',
      'Employees': 'พนักงาน',
      'Estimated profit': 'กำไรโดยประมาณ',
      'Error.': 'เกิดข้อผิดพลาด',
      'Export': 'ส่งออก',
      'Fair password.': 'รหัสผ่านพอใช้',
      'FollowUp': 'ติดตามผล',
      'Forgot password?': 'ลืมรหัสผ่าน?',
      'General': 'ทั่วไป',
      'Good': 'ปกติ',
      'Good password.': 'รหัสผ่านดี',
      'Grand Total': 'ยอดรวมสุทธิ',
      'Gross sales': 'ยอดขายรวม',
      'Held sales': 'บิลพักไว้',
      'Hide note': 'ซ่อนบันทึก',
      'Hold': 'ระงับ',
      'Important': 'สำคัญ',
      'Important Notes': 'บันทึกสำคัญ',
      'Inventory': 'คลังสินค้า',
      'Inventory Overview': 'ภาพรวมคลังสินค้า',
      'Inventory Report': 'รายงานคลังสินค้า',
      'Invoice': 'เลขที่บิล',
      'Language': 'ภาษา',
      'Line Total': 'ยอดรวมรายการ',
      'Load History': 'โหลดประวัติ',
      'Location': 'ที่ตั้ง',
      'Locations': 'ที่ตั้งสินค้า',
      'Login': 'เข้าสู่ระบบ',
      'Logout': 'ออกจากระบบ',
      'Low stock': 'สต็อกต่ำ',
      'Low Stock Products': 'สินค้าใกล้หมด',
      'Loyalty Point History': 'ประวัติคะแนนสะสม',
      'Loyalty Points': 'คะแนนสะสม',
      'Manage Customers': 'จัดการลูกค้า',
      'Manage Products': 'จัดการสินค้า',
      'Manage Stock': 'จัดการสต็อก',
      'Manager / Owner': 'ผู้จัดการ / เจ้าของ',
      'Manager / Owner Dashboard': 'แดชบอร์ดผู้จัดการ / เจ้าของ',
      'Margin': 'อัตรากำไร',
      'Member Level': 'ระดับสมาชิก',
      'Member Level History': 'ประวัติระดับสมาชิก',
      'Members': 'สมาชิก',
      'Method': 'วิธีชำระเงิน',
      'Mixed': 'ผสม',
      'Name': 'ชื่อ',
      'Net sales': 'ยอดขายสุทธิ',
      'New this month': 'ลูกค้าใหม่เดือนนี้',
      'New user': 'ผู้ใช้ใหม่',
      'No customer credit sales today.': 'วันนี้ไม่มีรายการขายเครดิตลูกค้า',
      'No customers found': 'ไม่พบลูกค้า',
      'No point history': 'ไม่มีประวัติคะแนน',
      'No refund history': 'ไม่มีประวัติคืนเงิน',
      'No product found': 'ไม่พบสินค้า',
      'Note Text': 'ข้อความบันทึก',
      'Note Type': 'ประเภทบันทึก',
      'Notes': 'บันทึก',
      'Notifications': 'การแจ้งเตือน',
      'Open Cash Drawer': 'เปิดลิ้นชักเก็บเงิน',
      'Open Customer Display': 'เปิดจอลูกค้า',
      'Other': 'อื่นๆ',
      'Out of stock': 'สินค้าหมด',
      'Outstanding Credit': 'เครดิตค้างชำระ',
      'Overdue Amount': 'ยอดค้างเกินกำหนด',
      'Paid Amount': 'ยอดที่ชำระ',
      'Password': 'รหัสผ่าน',
      'Payment': 'การชำระเงิน',
      'Payment Failed': 'ชำระเงินไม่สำเร็จ',
      'Payment History': 'ประวัติการชำระเงิน',
      'Payment Summary': 'สรุปการชำระเงิน',
      'Payment Successful': 'ชำระเงินสำเร็จ',
      'Pending Tasks': 'งานที่รอดำเนินการ',
      'Permissions': 'สิทธิ์การใช้งาน',
      'Points In': 'คะแนนเข้า',
      'Points Out': 'คะแนนออก',
      'Price': 'ราคา',
      'Print': 'พิมพ์',
      'Print Last Receipt': 'พิมพ์ใบเสร็จล่าสุด',
      'Product': 'สินค้า',
      'Products': 'สินค้า',
      'Profit / Revenue': 'กำไร / รายได้',
      'Purchase Count': 'จำนวนครั้งที่ซื้อ',
      'Purchase History': 'ประวัติการซื้อ',
      'QR': 'QR',
      'QR / payment': 'QR / ชำระเงิน',
      'Qty': 'จำนวน',
      'Quick Actions': 'เมนูลัด',
      'Recorded': 'บันทึกแล้ว',
      'Reference': 'อ้างอิง',
      'Refund Date': 'วันที่คืนเงิน',
      'Refund History': 'ประวัติคืนเงิน',
      'Refund No': 'เลขที่คืนเงิน',
      'Refunds': 'คืนเงิน',
      'Remaining': 'คงเหลือ',
      'Remark': 'หมายเหตุ',
      'Request ID:': 'รหัสคำขอ:',
      'Require Manager Approval': 'ต้องให้ผู้จัดการอนุมัติ',
      'Reset': 'รีเซ็ต',
      'Reset password': 'รีเซ็ตรหัสผ่าน',
      'Retail POS': 'ขายปลีก POS',
      'Return / Refund': 'คืนสินค้า / คืนเงิน',
      'Returns / Refunds': 'คืนสินค้า / คืนเงิน',
      'Role': 'บทบาท',
      'Roles': 'บทบาท',
      'Sale No': 'เลขที่ขาย',
      'Sales': 'การขาย',
      'Sales History': 'ประวัติการขาย',
      'Sales Report': 'รายงานการขาย',
      'Sales Trend': 'แนวโน้มยอดขาย',
      'Save': 'บันทึก',
      'Save Changes': 'บันทึกการเปลี่ยนแปลง',
      'Save Credit': 'บันทึกเครดิต',
      'Save Customer': 'บันทึกลูกค้า',
      'Save Note': 'บันทึก',
      'Save changes': 'บันทึกการเปลี่ยนแปลง',
      'Scanning Items': 'กำลังสแกนสินค้า',
      'Search': 'ค้นหา',
      'Search customer': 'ค้นหาลูกค้า',
      'Select customer': 'เลือกลูกค้า',
      'Select customer before viewing history': 'เลือกลูกค้าก่อนดูประวัติ',
      'Service': 'บริการ',
      'Set Credit': 'ตั้งค่าเครดิต',
      'Settings': 'ตั้งค่า',
      'Shortage': 'ขาด',
      'Sign In': 'เข้าสู่ระบบ',
      'Start New Sale': 'เริ่มขายใหม่',
      'Status': 'สถานะ',
      'Stock Adjustment': 'ปรับสต็อก',
      'Stock Count': 'ตรวจนับสต็อก',
      'Stock Movement': 'ความเคลื่อนไหวสต็อก',
      'Stock Transfer': 'โอนย้ายสต็อก',
      'Stock movement': 'ความเคลื่อนไหวสต็อก',
      'Stock value': 'มูลค่าสต็อก',
      'Strong password.': 'รหัสผ่านแข็งแรง',
      'Terminal': 'เครื่องขาย',
      'This Month': 'เดือนนี้',
      'This Week': 'สัปดาห์นี้',
      "Today's Sales": 'ยอดขายวันนี้',
      'Today sales': 'ยอดขายวันนี้',
      'Today’s Shift Summary': 'สรุปกะวันนี้',
      'Total': 'รวม',
      'Total Spending': 'ยอดใช้จ่ายรวม',
      'Total customers': 'ลูกค้าทั้งหมด',
      'Total products': 'สินค้าทั้งหมด',
      'Transactions': 'จำนวนบิล',
      'Transfer': 'โอนเงิน',
      'Try another filter or add a customer.': 'ลองใช้ตัวกรองอื่นหรือเพิ่มลูกค้า',
      'Type': 'ประเภท',
      'Unit': 'หน่วย',
      'Unit Price': 'ราคาต่อหน่วย',
      'Username': 'ชื่อผู้ใช้',
      'Username or email': 'ชื่อผู้ใช้หรืออีเมล',
      'Users': 'ผู้ใช้',
      'Using credit': 'ใช้เครดิต',
      'View / Print': 'ดู / พิมพ์',
      'View All': 'ดูทั้งหมด',
      'View Cashier Report': 'ดูรายงานแคชเชียร์',
      'View Customer Report': 'ดูรายงานลูกค้า',
      'View Detail': 'ดูรายละเอียด',
      'View History': 'ดูประวัติ',
      'View Inventory Report': 'ดูรายงานคลังสินค้า',
      'View Points': 'ดูคะแนน',
      'View Sales Report': 'ดูรายงานการขาย',
      'Waiting for Payment': 'รอการชำระเงิน',
      'Waiting for payment.': 'รอการชำระเงิน',
      'Waiting for scanned items': 'รอรายการสินค้าที่สแกน',
      'Walk-in': 'ลูกค้าทั่วไป',
      'Walk-in Customer': 'ลูกค้าทั่วไป',
      'Warning': 'คำเตือน',
      'Weak password.': 'รหัสผ่านอ่อน',
      'Welcome. Your purchase details will appear here.': 'ยินดีต้อนรับ รายละเอียดการซื้อจะแสดงที่นี่',
      'Wholesale POS': 'ขายส่ง POS'
    }
  };

  Object.assign(phraseTranslations.th, {
    'Add Payment': 'เพิ่มการชำระเงิน',
    'Add items or enter a payment amount first.': 'เพิ่มสินค้า หรือกรอกยอดชำระก่อน',
    'Available credit': 'เครดิตที่ใช้ได้',
    'Available credit is not enough for the full sale. Remaining balance must be paid by another method.': 'เครดิตที่ใช้ได้ไม่พอสำหรับทั้งรายการ ต้องชำระยอดคงเหลือด้วยวิธีอื่น',
    'Ask for phone number or name, then select the member.': 'ขอเบอร์โทรหรือชื่อ แล้วเลือกลูกค้าสมาชิก',
    'Balance Due': 'ยอดค้างชำระ',
    'Barcode lookup failed.': 'ค้นหาบาร์โค้ดไม่สำเร็จ',
    'Barcode scan': 'สแกนบาร์โค้ด',
    'Cart is empty.': 'ตะกร้าว่าง',
    'Choose a member to review credit for this sale.': 'เลือกลูกค้าเพื่อตรวจสอบเครดิตสำหรับรายการนี้',
    'Clear Cart': 'ล้างตะกร้า',
    'Complete the sale first, then print the invoice.': 'จบการขายก่อน แล้วจึงพิมพ์ใบกำกับ',
    'Confirm Hold': 'ยืนยันพักบิล',
    'Confirm Payment': 'ยืนยันการชำระเงิน',
    'Credit Applied': 'ใช้เครดิตแล้ว',
    'Credit Note:': 'หมายเหตุเครดิต:',
    'Credit Used': 'ใช้เครดิตแล้ว',
    'Credit is not available for this customer.': 'ลูกค้ารายนี้ไม่สามารถใช้เครดิตได้',
    'Credit is not enough. Remaining balance must be paid by another method.': 'เครดิตไม่พอ ต้องชำระยอดคงเหลือด้วยวิธีอื่น',
    'Credit/Debit Card': 'บัตรเครดิต/เดบิต',
    'Customer credit is not enough for this payment.': 'เครดิตลูกค้าไม่พอสำหรับการชำระนี้',
    'Customer credit payment requires a selected member customer.': 'การชำระด้วยเครดิตลูกค้าต้องเลือกลูกค้าสมาชิก',
    'Customer credit used': 'ใช้เครดิตลูกค้า',
    'Customer credit used in this sale becomes customer debt and must be repaid later.': 'เครดิตลูกค้าที่ใช้ในรายการนี้จะเป็นหนี้ที่ต้องชำระภายหลัง',
    'Customer credit used in this sale will become customer debt and must be repaid later.': 'เครดิตลูกค้าที่ใช้ในรายการนี้จะเป็นหนี้ที่ต้องชำระภายหลัง',
    'Customer credit will be used for this sale. This becomes customer debt to repay later.': 'รายการนี้จะใช้เครดิตลูกค้า และจะเป็นหนี้ที่ต้องชำระภายหลัง',
    'Customer name, reason, or pickup note': 'ชื่อลูกค้า เหตุผล หรือหมายเหตุรับสินค้า',
    'Display': 'แสดงผล',
    'Enough credit for this sale.': 'เครดิตเพียงพอสำหรับรายการขายนี้',
    'Enter member phone number or name': 'กรอกเบอร์โทรหรือชื่อลูกค้าสมาชิก',
    'Estimated total': 'ยอดรวมโดยประมาณ',
    'Fully Paid': 'ชำระครบแล้ว',
    'Fully paid by customer credit': 'ชำระครบด้วยเครดิตลูกค้า',
    'Generate': 'สร้าง',
    'Generate QR for the remaining balance.': 'สร้าง QR สำหรับยอดคงเหลือ',
    'Generating...': 'กำลังสร้าง...',
    'Hold Sales': 'บิลพักไว้',
    'Hold note': 'หมายเหตุบิลพัก',
    'Invoice is not ready to print.': 'ใบกำกับยังไม่พร้อมพิมพ์',
    'Item': 'รายการ',
    'Item Discount': 'ส่วนลดสินค้า',
    'Member customer': 'ลูกค้าสมาชิก',
    'Member customer?': 'เป็นลูกค้าสมาชิก?',
    'Net Amount': 'ยอดสุทธิ',
    'Net amount': 'ยอดสุทธิ',
    'No': 'ไม่',
    'No active members found. Continue as walk-in customer.': 'ไม่พบสมาชิกที่ใช้งานอยู่ ดำเนินการต่อเป็นลูกค้าทั่วไป',
    'No code': 'ไม่มีรหัส',
    'No level': 'ไม่มีระดับ',
    'No payment methods found': 'ไม่พบวิธีชำระเงิน',
    'No payments recorded.': 'ยังไม่มีการบันทึกการชำระเงิน',
    'No phone': 'ไม่มีเบอร์โทร',
    'No products found': 'ไม่พบสินค้า',
    'Other Payment': 'ยอดชำระอื่น',
    'Other payment amount': 'ยอดชำระอื่น',
    'Paid': 'ชำระแล้ว',
    'Paid amount': 'ยอดที่ชำระ',
    'Payment Ready': 'พร้อมชำระเงิน',
    'Payment amount': 'ยอดชำระ',
    'Payment amount must be greater than or equal to net amount.': 'ยอดชำระต้องมากกว่าหรือเท่ากับยอดสุทธิ',
    'Payment covers this sale': 'ยอดชำระครอบคลุมรายการขายนี้',
    'Payment is incomplete. Remaining balance must be paid before completing sale.': 'การชำระเงินยังไม่ครบ ต้องชำระยอดคงเหลือก่อนจบการขาย',
    'Please review the failure reason and payment summary.': 'โปรดตรวจสอบสาเหตุที่ไม่สำเร็จและสรุปการชำระเงิน',
    'POS Checkout': 'หน้าขาย POS',
    'Point of Sale Receipt / Invoice': 'ใบเสร็จ / ใบกำกับจากระบบขายหน้าร้าน',
    'Processing payment...': 'กำลังดำเนินการชำระเงิน...',
    'Product added to cart.': 'เพิ่มสินค้าลงตะกร้าแล้ว',
    'Product is inactive.': 'สินค้านี้ไม่ได้ใช้งาน',
    'Product not found for this barcode.': 'ไม่พบสินค้าสำหรับบาร์โค้ดนี้',
    'Product search': 'ค้นหาสินค้า',
    'Product VAT': 'VAT สินค้า',
    'PromptPay QR': 'PromptPay QR',
    'PromptPay QR Code': 'PromptPay QR Code',
    'QR Payment': 'ชำระด้วย QR',
    'Receipt': 'ใบเสร็จ',
    'Receipt / Invoice': 'ใบเสร็จ / ใบกำกับ',
    'Reference no': 'เลขอ้างอิง',
    'Reference number is required.': 'กรุณากรอกเลขอ้างอิง',
    'Remaining Balance': 'ยอดคงเหลือ',
    'Remaining balance': 'ยอดคงเหลือ',
    'Remaining balance must be collected': 'ต้องรับชำระยอดคงเหลือ',
    'Remaining balance must be paid by another method.': 'ต้องชำระยอดคงเหลือด้วยวิธีอื่น',
    'Review items before payment': 'ตรวจสอบรายการก่อนชำระเงิน',
    'Review payment details before completing this sale.': 'ตรวจสอบรายละเอียดการชำระเงินก่อนจบการขาย',
    'Review totals': 'ตรวจสอบยอดรวม',
    'Sale Information': 'ข้อมูลการขาย',
    'Sale completed successfully.': 'ขายสำเร็จ',
    'Sale failed. Please check payment details.': 'การขายไม่สำเร็จ โปรดตรวจสอบรายละเอียดการชำระเงิน',
    'Scan barcode and press Enter': 'สแกนบาร์โค้ดแล้วกด Enter',
    'Scan barcode or select products to start a sale.': 'สแกนบาร์โค้ดหรือเลือกสินค้าเพื่อเริ่มขาย',
    'Scan products, manage cart, accept payment, and complete sales quickly.': 'สแกนสินค้า จัดการตะกร้า รับชำระเงิน และจบการขายได้อย่างรวดเร็ว',
    'Search name, code, or barcode': 'ค้นหาชื่อ รหัส หรือบาร์โค้ด',
    'Select Another Payment': 'เลือกวิธีชำระเงินอื่น',
    'Selected customer': 'ลูกค้าที่เลือก',
    'Thank you.': 'ขอบคุณ',
    'Try a different scan or search term.': 'ลองสแกนใหม่หรือค้นหาด้วยคำอื่น',
    'Use Available Credit': 'ใช้เครดิตที่มี',
    'Use Credit for Full Payment': 'ใช้เครดิตชำระเต็มจำนวน',
    'Use Partial Credit': 'ใช้เครดิตบางส่วน',
    'Voucher': 'คูปอง',
    'Yes': 'ใช่'
  });

  Object.assign(phraseTranslations.th, {
    'All': 'ทั้งหมด',
    'Additional tax': 'ภาษีเพิ่มเติม',
    'Change': 'เงินทอน',
    'Change / Missing': 'เงินทอน / ยอดขาด',
    'Hold Sale': 'พักบิล',
    'In stock': 'มีสต็อก',
    'Item discount': 'ส่วนลดสินค้า',
    'Order discount': 'ส่วนลดท้ายบิล',
    'Pay Now': 'ชำระเงิน',
    'Payment method': 'วิธีชำระเงิน',
    'Subtotal': 'ยอดรวมก่อนหักส่วนลด'
    ,
    'Your cart is empty': 'ตะกร้าของคุณว่าง'
  });

  Object.assign(translations.en, {
    'nav.rubberPayBill': 'Pay Rubber Bills',
    'nav.rubberPlantation': 'Rubber Plantation',
    'nav.rubberPurchases': 'Rubber Purchases',
    'rubberDisplay.amountToSeller': 'Amount to seller',
    'rubberDisplay.billSelected': 'Bill selected',
    'rubberDisplay.confirmPayment': 'Confirm payment',
    'rubberDisplay.creditDeducted': 'Credit deducted',
    'rubberDisplay.deduction': 'Deduction',
    'rubberDisplay.failed': 'Failed',
    'rubberDisplay.fullBillAmount': 'Full bill amount',
    'rubberDisplay.paymentMethod': 'Payment method',
    'rubberDisplay.paymentSummary': 'Payment Summary',
    'rubberDisplay.pricePerKg': 'Price per kg',
    'rubberDisplay.processing': 'Processing',
    'rubberDisplay.purchaseDate': 'Purchase date',
    'rubberDisplay.purchaseLocation': 'Purchase location',
    'rubberDisplay.receiptNumber': 'Receipt number',
    'rubberDisplay.rubberBillPayment': 'Rubber Bill Payment',
    'rubberDisplay.sellerInformation': 'Seller Information',
    'rubberDisplay.subtitle': 'Please confirm the bill with the cashier.',
    'rubberDisplay.weight': 'Weight',
    'rubberPay.billDate': 'Purchase date',
    'rubberPay.billList': 'Bills to pay',
    'rubberPay.billListHelp': 'Search by member code, seller name, or phone number.',
    'rubberPay.cash': 'Cash',
    'rubberPay.cashHelp': 'Receive cash and print receipt immediately.',
    'rubberPay.confirm': 'Confirm payment',
    'rubberPay.customerDisplay': 'Customer Display',
    'rubberPay.deduction': 'Deduction',
    'rubberPay.dueBills': 'Due bills',
    'rubberPay.fullAmount': 'Full amount to pay',
    'rubberPay.fullPaymentOnly': 'This cashier flow accepts full payment only.',
    'rubberPay.latestReceipt': 'Latest receipt',
    'rubberPay.location': 'Purchase location',
    'rubberPay.noBillSelected': 'Select a bill to pay.',
    'rubberPay.noDueBills': 'No due rubber bills.',
    'rubberPay.outstandingTotal': 'Total outstanding',
    'rubberPay.payFull': 'Pay full bill',
    'rubberPay.payment': 'Payment',
    'rubberPay.paymentHelp': 'Review the bill amount, take payment, and print the receipt.',
    'rubberPay.paymentHistory': 'Payment history',
    'rubberPay.paymentMethod': 'Payment method',
    'rubberPay.price': 'Price',
    'rubberPay.receiptQuestion': 'Print receipt?',
    'rubberPay.remark': 'Remark',
    'rubberPay.seller': 'Seller',
    'rubberPay.subtitle': 'Select a rubber purchase bill, record payment, and print the receipt.',
    'rubberPay.title': 'Pay Rubber Bills',
    'rubberPay.transfer': 'Bank transfer',
    'rubberPay.transferHelp': 'Record the transfer reference in the remark.',
    'rubberPay.weight': 'Weight'
  });

  Object.assign(translations.th, {
    'nav.rubberPayBill': 'จ่ายบิลยางพารา',
    'nav.rubberPlantation': 'สวนยางพารา',
    'nav.rubberPurchases': 'รายการรับซื้อยาง',
    'rubberDisplay.amountToSeller': 'ยอดจ่ายให้ผู้ขาย',
    'rubberDisplay.billSelected': 'เลือกบิลแล้ว',
    'rubberDisplay.confirmPayment': 'ยืนยันการจ่ายบิล',
    'rubberDisplay.creditDeducted': 'หักเครดิต',
    'rubberDisplay.deduction': 'หัก',
    'rubberDisplay.failed': 'ไม่สำเร็จ',
    'rubberDisplay.fullBillAmount': 'ยอดเต็มของบิล',
    'rubberDisplay.paymentMethod': 'วิธีชำระเงิน',
    'rubberDisplay.paymentSummary': 'สรุปการชำระเงิน',
    'rubberDisplay.pricePerKg': 'ราคาต่อกก.',
    'rubberDisplay.processing': 'กำลังดำเนินการ',
    'rubberDisplay.purchaseDate': 'วันที่รับซื้อ',
    'rubberDisplay.purchaseLocation': 'สถานที่รับซื้อ',
    'rubberDisplay.receiptNumber': 'เลขที่ใบเสร็จ',
    'rubberDisplay.rubberBillPayment': 'จ่ายบิลยางพารา',
    'rubberDisplay.sellerInformation': 'ข้อมูลผู้ขาย',
    'rubberDisplay.subtitle': 'กรุณายืนยันบิลกับแคชเชียร์',
    'rubberDisplay.weight': 'น้ำหนัก',
    'rubberPay.billDate': 'วันที่รับซื้อ',
    'rubberPay.billList': 'บิลที่รอชำระ',
    'rubberPay.billListHelp': 'ค้นหาจากรหัสสมาชิก ชื่อผู้ขาย หรือเบอร์โทร',
    'rubberPay.cash': 'เงินสด',
    'rubberPay.cashHelp': 'รับเงินสดและออกใบเสร็จทันที',
    'rubberPay.confirm': 'ยืนยันการจ่ายบิล',
    'rubberPay.customerDisplay': 'จอลูกค้า',
    'rubberPay.deduction': 'หัก',
    'rubberPay.dueBills': 'บิลรอจ่าย',
    'rubberPay.fullAmount': 'ยอดที่ต้องจ่ายเต็มจำนวน',
    'rubberPay.fullPaymentOnly': 'ระบบนี้จ่ายเต็มจำนวนเท่านั้น',
    'rubberPay.latestReceipt': 'ใบเสร็จล่าสุด',
    'rubberPay.location': 'สถานที่รับซื้อ',
    'rubberPay.noBillSelected': 'เลือกบิลเพื่อชำระเงิน',
    'rubberPay.noDueBills': 'ไม่มีบิลค้างชำระ',
    'rubberPay.outstandingTotal': 'ยอดค้างรวม',
    'rubberPay.payFull': 'จ่ายบิลเต็มจำนวน',
    'rubberPay.payment': 'ชำระเงิน',
    'rubberPay.paymentHelp': 'ตรวจสอบยอดก่อนบันทึกและพิมพ์ใบเสร็จ',
    'rubberPay.paymentHistory': 'ประวัติการจ่ายเงิน',
    'rubberPay.paymentMethod': 'วิธีชำระเงิน',
    'rubberPay.price': 'ราคา',
    'rubberPay.receiptQuestion': 'ต้องการพิมพ์ใบเสร็จไหม?',
    'rubberPay.remark': 'หมายเหตุ',
    'rubberPay.seller': 'ผู้ขาย',
    'rubberPay.subtitle': 'เลือกบิลรับซื้อยาง บันทึกการชำระเงิน และพิมพ์ใบเสร็จรับเงิน',
    'rubberPay.title': 'จ่ายบิลยางพารา',
    'rubberPay.transfer': 'โอนบัญชี',
    'rubberPay.transferHelp': 'บันทึกเลขอ้างอิงในหมายเหตุ',
    'rubberPay.weight': 'น้ำหนัก'
  });

  const getLanguage = () => {
    const storedLanguage = localStorage.getItem(languageKey);
    return storedLanguage && translations[storedLanguage] ? storedLanguage : fallbackLanguage;
  };

  const shouldSkipTextNode = (node) => {
    const parent = node.parentElement;
    if (!parent) return true;
    return Boolean(parent.closest('script, style, noscript, code, pre, textarea, [data-no-translate], [data-i18n]'));
  };

  const translatePhrase = (value, language) => {
    if (language === fallbackLanguage) return value;
    const phrases = phraseTranslations[language] || {};
    const trimmed = value.replace(/\s+/g, ' ').trim();
    if (!trimmed) return value;

    if (phrases[trimmed]) return value.replace(trimmed, phrases[trimmed]);

    const itemMatch = trimmed.match(/^(\d+)\s+items?$/i);
    if (itemMatch) return value.replace(trimmed, `${itemMatch[1]} รายการ`);

    const memberMatch = trimmed.match(/^(\d+)\s+member\s+matches?\s+found\.$/i);
    if (memberMatch) return value.replace(trimmed, `พบสมาชิก ${memberMatch[1]} รายการ`);

    const stockMatch = trimmed.match(/^Stock\s+(.+)$/i);
    if (stockMatch) return value.replace(trimmed, `สต็อก ${stockMatch[1]}`);

    const lowStockMatch = trimmed.match(/^Low\s+(.+)$/i);
    if (lowStockMatch) return value.replace(trimmed, `ต่ำ ${lowStockMatch[1]}`);

    const saleCompletedMatch = trimmed.match(/^Sale\s+(.+)\s+completed\.?$/i);
    if (saleCompletedMatch) return value.replace(trimmed, `บิล ${saleCompletedMatch[1]} เสร็จสมบูรณ์`);

    const startedMatch = trimmed.match(/^Started\s+(.+)$/i);
    if (startedMatch) return value.replace(trimmed, `เริ่ม ${startedMatch[1]}`);

    const terminalMatch = trimmed.match(/^Terminal:\s*(.+)$/i);
    if (terminalMatch) return value.replace(trimmed, `เครื่องขาย: ${terminalMatch[1]}`);

    const statusMatch = trimmed.match(/^Status:\s*(.+)$/i);
    if (statusMatch) {
      const status = phrases[statusMatch[1]] || statusMatch[1];
      return value.replace(trimmed, `สถานะ: ${status}`);
    }

    const saleMatch = trimmed.match(/^Sale\s+(.+)$/i);
    if (saleMatch) return value.replace(trimmed, `บิล ${saleMatch[1]}`);

    return value;
  };

  const isTranslatableSource = (value) => {
    const trimmed = value.replace(/\s+/g, ' ').trim();
    if (!trimmed) return false;
    return Boolean(
      phraseTranslations.th?.[trimmed] ||
      /^(\d+)\s+items?$/i.test(trimmed) ||
      /^(\d+)\s+member\s+matches?\s+found\.$/i.test(trimmed) ||
      /^Stock\s+(.+)$/i.test(trimmed) ||
      /^Low\s+(.+)$/i.test(trimmed) ||
      /^Sale\s+(.+)\s+completed\.?$/i.test(trimmed) ||
      /^Started\s+(.+)$/i.test(trimmed) ||
      /^Terminal:\s*(.+)$/i.test(trimmed) ||
      /^Status:\s*(.+)$/i.test(trimmed) ||
      /^Sale\s+(.+)$/i.test(trimmed)
    );
  };

  const refreshOriginals = (root = document.body) => {
    if (!root) return;
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
    const nodes = [];
    while (walker.nextNode()) nodes.push(walker.currentNode);
    nodes.forEach((node) => {
      if (isTranslatableSource(node.nodeValue || '')) originalText.delete(node);
    });
    root.querySelectorAll?.('[placeholder], [aria-label], [title], [data-bs-original-title], input[type="button"], input[type="submit"], input[type="reset"]').forEach((element) => {
      const originals = originalAttributes.get(element);
      if (!originals) return;
      Object.keys(originals).forEach((attribute) => {
        if (isTranslatableSource(element.getAttribute(attribute) || '')) delete originals[attribute];
      });
    });
  };

  const translateTextNode = (node, language) => {
    if (shouldSkipTextNode(node)) return;
    if (!originalText.has(node)) originalText.set(node, node.nodeValue);
    const source = originalText.get(node);
    const translated = translatePhrase(source, language);
    if (node.nodeValue !== translated) node.nodeValue = translated;
  };

  const translateElementAttributes = (element, language) => {
    if (element.closest('[data-no-translate]')) return;
    const attributes = ['placeholder', 'aria-label', 'title', 'data-bs-original-title'];
    if (element.matches('input[type="button"], input[type="submit"], input[type="reset"]')) {
      attributes.push('value');
    }

    attributes.forEach((attribute) => {
      if (!element.hasAttribute(attribute)) return;
      let originals = originalAttributes.get(element);
      if (!originals) {
        originals = {};
        originalAttributes.set(element, originals);
      }
      if (!Object.prototype.hasOwnProperty.call(originals, attribute)) {
        originals[attribute] = element.getAttribute(attribute);
      }
      const translated = translatePhrase(originals[attribute], language);
      if (element.getAttribute(attribute) !== translated) element.setAttribute(attribute, translated);
    });
  };

  const translatePagePhrases = (language) => {
    const root = document.body;
    if (!root) return;

    const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
    const nodes = [];
    while (walker.nextNode()) nodes.push(walker.currentNode);
    nodes.forEach((node) => translateTextNode(node, language));
    root.querySelectorAll('[placeholder], [aria-label], [title], [data-bs-original-title], input[type="button"], input[type="submit"], input[type="reset"]').forEach((element) => {
      translateElementAttributes(element, language);
    });
  };

  const setLanguage = (language) => {
    const nextLanguage = translations[language] ? language : fallbackLanguage;
    const copy = translations[nextLanguage];
    isApplyingLanguage = true;

    document.documentElement.lang = nextLanguage;
    document.documentElement.dataset.appLanguage = nextLanguage;

    document.querySelectorAll('[data-i18n]').forEach((element) => {
      const key = element.dataset.i18n;
      if (!key || !copy[key]) return;
      element.textContent = copy[key];
    });

    document.querySelectorAll('[data-i18n-title]').forEach((element) => {
      const value = copy[element.dataset.i18nTitle];
      if (!value) return;
      element.setAttribute('title', value);
      element.setAttribute('data-bs-original-title', value);
    });

    document.querySelectorAll('[data-language-option]').forEach((button) => {
      const isActive = button.dataset.languageOption === nextLanguage;
      button.classList.toggle('active', isActive);
      button.setAttribute('aria-pressed', isActive ? 'true' : 'false');
    });

    translatePagePhrases(nextLanguage);
    localStorage.setItem(languageKey, nextLanguage);
    window.dispatchEvent(new CustomEvent('aphiwatpos:languagechange', { detail: { language: nextLanguage } }));
    setTimeout(() => { isApplyingLanguage = false; }, 0);
  };

  window.AphiwatPOSLanguage = {
    apply: (root = document.body) => {
      refreshOriginals(root);
      setLanguage(getLanguage());
    },
    current: getLanguage,
    translate: (value) => translatePhrase(value, getLanguage())
  };

  if (shell && localStorage.getItem(collapsedKey) === 'true') {
    shell.classList.add('sidebar-collapsed');
  }

  sidebarToggle?.addEventListener('click', () => {
    shell?.classList.toggle('sidebar-collapsed');
    localStorage.setItem(collapsedKey, shell?.classList.contains('sidebar-collapsed') ? 'true' : 'false');
  });

  if (window.bootstrap) {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach((element) => {
      bootstrap.Tooltip.getOrCreateInstance(element, {
        trigger: 'hover',
        boundary: document.body
      });
    });
  }

  document.querySelectorAll('[data-language-option]').forEach((button) => {
    button.addEventListener('click', () => setLanguage(button.dataset.languageOption));
  });

  setLanguage(getLanguage());

  if (document.body) {
    const observer = new MutationObserver((mutations) => {
      if (isApplyingLanguage) return;
      if (!mutations.some((mutation) => mutation.type === 'childList' || mutation.type === 'characterData' || mutation.type === 'attributes')) return;
      mutations.forEach((mutation) => {
        if (mutation.type === 'characterData') originalText.delete(mutation.target);
        if (mutation.type === 'attributes') {
          const originals = originalAttributes.get(mutation.target);
          if (originals) delete originals[mutation.attributeName];
        }
      });
      setLanguage(getLanguage());
    });
    observer.observe(document.body, {
      attributes: true,
      attributeFilter: ['placeholder', 'aria-label', 'title', 'data-bs-original-title', 'value'],
      childList: true,
      characterData: true,
      subtree: true
    });
  }

  document.querySelectorAll('[data-password-strength-input]').forEach((passwordInput) => {
    const container = passwordInput.closest('form') || document;
    const strengthBar = container.querySelector('[data-password-strength-bar]');
    const strengthText = container.querySelector('[data-password-strength-text]');

    passwordInput.addEventListener('input', () => {
      const value = passwordInput.value;
      let score = 0;
      if (value.length >= 8) score += 1;
      if (/[A-Z]/.test(value)) score += 1;
      if (/[0-9]/.test(value)) score += 1;
      if (/[^A-Za-z0-9]/.test(value)) score += 1;

      const states = [
        { width: '0%', color: '#dc2626', text: 'Enter at least 8 characters.' },
        { width: '28%', color: '#dc2626', text: 'Weak password.' },
        { width: '55%', color: '#f59e0b', text: 'Fair password.' },
        { width: '78%', color: '#059669', text: 'Good password.' },
        { width: '100%', color: '#047857', text: 'Strong password.' }
      ];
      const state = states[score];

      if (strengthBar) {
        strengthBar.style.width = state.width;
        strengthBar.style.backgroundColor = state.color;
      }

      if (strengthText) {
        strengthText.textContent = state.text;
      }
    });
  });

  document.querySelectorAll('[data-image-input]').forEach((input) => {
    input.addEventListener('change', () => {
      const preview = document.querySelector(`[data-image-preview="${input.dataset.imageInput}"]`);
      const file = input.files?.[0];
      if (!preview || !file) return;

      const image = document.createElement('img');
      image.alt = 'Profile image preview';
      image.src = URL.createObjectURL(file);
      image.onload = () => URL.revokeObjectURL(image.src);
      preview.replaceChildren(image);
    });
  });
})();
document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll("[data-image-input]").forEach((input) => {
    input.addEventListener("change", () => {
      const target = document.querySelector(`[data-image-preview="${input.dataset.imageInput}"]`);
      const file = input.files && input.files[0];
      if (!target || !file) return;
      const reader = new FileReader();
      reader.onload = (event) => {
        target.innerHTML = "";
        const img = document.createElement("img");
        img.src = event.target.result;
        img.alt = "Selected image preview";
        target.appendChild(img);
      };
      reader.readAsDataURL(file);
    });
  });

  document.querySelectorAll("[data-price-form]").forEach((form) => {
    const cost = form.querySelector("[data-new-cost]");
    const selling = form.querySelector("[data-new-selling]");
    const amount = form.querySelector("[data-profit-amount]");
    const margin = form.querySelector("[data-profit-margin]");
    const sellingError = form.querySelector("[data-new-selling-error]");
    const minimumSellingPrice = Number.parseFloat(form.dataset.minimumSellingPrice || "0") || 0;
    const saveButton = form.querySelector('button[type="submit"]');

    const update = () => {
      const c = Number.parseFloat(cost?.value || "0");
      const s = Number.parseFloat(selling?.value || "0");
      const profit = s - c;
      const pct = s <= 0 ? 0 : (profit / s) * 100;
      const valid = s >= minimumSellingPrice;
      if (amount) amount.value = profit.toFixed(2);
      if (margin) margin.value = `${pct.toFixed(2)}%`;
      selling?.classList.toggle("is-invalid", !valid);
      sellingError?.classList.toggle("d-none", valid);
      if (saveButton) saveButton.disabled = !valid;
      return valid;
    };

    cost?.addEventListener("input", update);
    selling?.addEventListener("input", update);
    form.addEventListener("submit", (event) => {
      if (update()) return;
      event.preventDefault();
      selling?.focus();
    });
    update();
  });

  document.querySelectorAll("[data-pricing-section]").forEach((section) => {
    const form = section.closest("form");
    const minimumCost = section.querySelector("[data-minimum-cost]");
    const vatPercentage = section.querySelector("[data-vat-percentage]");
    const vatAmount = section.querySelector("[data-vat-amount]");
    const taxRate = section.querySelector("[data-tax-rate]");
    const minimumSellingPrice = section.querySelector("[data-minimum-selling-price]");
    const minimumSellingDisplay = section.querySelector("[data-minimum-selling-display]");
    const sellingPrice = section.querySelector("[data-selling-price]");
    const error = section.querySelector("[data-selling-price-error]");
    const saveButton = form?.querySelector('button[type="submit"]');

    const updatePricing = () => {
      const minimumCostValue = Number.parseFloat(minimumCost?.value || "0") || 0;
      const vatPercentageValue = Number.parseFloat(vatPercentage?.value || "0") || 0;
      const sellingPriceValue = Number.parseFloat(sellingPrice?.value || "0") || 0;
      const vatAmountValue = minimumCostValue * vatPercentageValue / 100;
      const minimumSellingValue = minimumCostValue + vatAmountValue;
      const isValid = sellingPriceValue >= minimumSellingValue;

      if (vatAmount) vatAmount.value = vatAmountValue.toFixed(2);
      if (minimumSellingPrice) minimumSellingPrice.value = minimumSellingValue.toFixed(2);
      if (minimumSellingDisplay) minimumSellingDisplay.textContent = minimumSellingValue.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
      if (taxRate) taxRate.value = vatPercentageValue.toFixed(2);
      sellingPrice?.classList.toggle("is-invalid", !isValid);
      error?.classList.toggle("d-none", isValid);
      if (saveButton) saveButton.disabled = !isValid;
      return isValid;
    };

    [minimumCost, vatPercentage, sellingPrice].forEach((input) => input?.addEventListener("input", updatePricing));
    form?.addEventListener("submit", (event) => {
      if (updatePricing()) return;
      event.preventDefault();
      sellingPrice?.focus();
    });
    updatePricing();
  });

  document.querySelectorAll(".toast.show").forEach((toastElement) => {
    const toast = bootstrap.Toast.getOrCreateInstance(toastElement, { delay: 3600 });
    toast.show();
  });

  const parseNumber = (value) => Number.parseFloat(value || "0") || 0;

  document.querySelectorAll("[data-product-select]").forEach((select) => {
    const row = select.closest("tr");
    const updateStockFields = () => {
      const option = select.selectedOptions[0];
      const stock = parseNumber(option?.dataset.stock);
      const cost = parseNumber(option?.dataset.cost);
      row?.querySelectorAll("[data-current-qty], [data-system-qty], [data-available-qty]").forEach((input) => input.value = stock.toFixed(2));
      row?.querySelectorAll("[data-unit-cost]").forEach((input) => input.value = cost.toFixed(4));
      const unitLabel = row?.querySelector("[data-unit-label]");
      if (unitLabel) unitLabel.textContent = option?.dataset.unit || "-";
      row?.dispatchEvent(new Event("inventory:recalculate", { bubbles: true }));
    };
    select.addEventListener("change", updateStockFields);
    updateStockFields();
  });

  document.querySelectorAll("[data-stock-location-source]").forEach((locationSelect) => {
    const form = locationSelect.closest("form") || document;
    const handler = locationSelect.dataset.stockHandler || "?handler=ProductStock";
    const refreshStock = async () => {
      const locationId = locationSelect.value;
      const productSelects = form.querySelectorAll("[data-product-select]");

      if (!locationId) {
        productSelects.forEach((select) => {
          select.querySelectorAll("option[data-stock]").forEach((option) => option.dataset.stock = "0");
          select.dispatchEvent(new Event("change", { bubbles: true }));
        });
        return;
      }

      try {
        locationSelect.classList.add("is-loading");
        const separator = handler.includes("?") ? "&" : "?";
        const response = await fetch(`${handler}${separator}locationId=${encodeURIComponent(locationId)}`, {
          headers: { "Accept": "application/json" }
        });
        if (!response.ok) throw new Error("Stock lookup failed.");

        const rows = await response.json();
        const stockByProduct = new Map(rows.map((row) => [String(row.productId), row]));

        productSelects.forEach((select) => {
          select.querySelectorAll("option[value]").forEach((option) => {
            const stock = stockByProduct.get(option.value);
            if (!stock) return;
            option.dataset.stock = stock.currentStock ?? 0;
            option.dataset.cost = stock.unitCost ?? option.dataset.cost ?? 0;
            option.dataset.unit = stock.unit ?? option.dataset.unit ?? "";
          });
          select.dispatchEvent(new Event("change", { bubbles: true }));
        });
      } catch {
        productSelects.forEach((select) => select.dispatchEvent(new Event("change", { bubbles: true })));
      } finally {
        locationSelect.classList.remove("is-loading");
      }
    };

    locationSelect.addEventListener("change", refreshStock);
    if (locationSelect.value) refreshStock();
  });

  document.querySelectorAll("[data-adjustment-table] tbody tr").forEach((row) => {
    const update = () => {
      const current = parseNumber(row.querySelector("[data-current-qty]")?.value);
      const qty = parseNumber(row.querySelector("[data-adjustment-qty]")?.value);
      const type = row.querySelector("[data-adjustment-type]")?.value || "Increase";
      const next = type === "Decrease" ? current - qty : current + qty;
      const output = row.querySelector("[data-new-qty]");
      if (output) output.value = next.toFixed(2);
      row.classList.toggle("table-danger", next < 0);
    };
    row.addEventListener("inventory:recalculate", update);
    row.querySelectorAll("[data-adjustment-qty], [data-adjustment-type]").forEach((element) => element.addEventListener("input", update));
    update();
  });

  document.querySelectorAll("[data-count-table] tbody tr").forEach((row) => {
    const update = () => {
      const system = parseNumber(row.querySelector("[data-system-qty]")?.value);
      const counted = parseNumber(row.querySelector("[data-counted-qty]")?.value);
      const variance = counted - system;
      const output = row.querySelector("[data-variance-qty]");
      if (output) output.value = variance.toFixed(2);
      row.classList.toggle("table-warning", variance !== 0);
    };
    row.addEventListener("inventory:recalculate", update);
    row.querySelectorAll("[data-counted-qty]").forEach((element) => element.addEventListener("input", update));
    update();
  });

  document.querySelectorAll("[data-transfer-table] tbody tr").forEach((row) => {
    const update = () => {
      const available = parseNumber(row.querySelector("[data-available-qty]")?.value);
      const qty = parseNumber(row.querySelector("[data-transfer-qty]")?.value);
      row.classList.toggle("table-danger", qty > available && qty > 0);
    };
    row.addEventListener("inventory:recalculate", update);
    row.querySelectorAll("[data-transfer-qty]").forEach((element) => element.addEventListener("input", update));
    update();
  });

  document.querySelectorAll("[data-clear-row]").forEach((button) => {
    button.addEventListener("click", () => {
      const row = button.closest("tr");
      row?.querySelectorAll("input").forEach((input) => {
        if (!input.hasAttribute("readonly")) input.value = "";
      });
      row?.querySelectorAll("select").forEach((select) => select.selectedIndex = 0);
      row?.dispatchEvent(new Event("inventory:recalculate", { bubbles: true }));
    });
  });

  document.querySelectorAll("[data-barcode-scan-input]").forEach((input) => {
    const setFeedback = (message, isError = false) => {
      const form = input.closest("form") || document;
      const feedback = form.querySelector("[data-scan-feedback]");
      if (!feedback) return;
      feedback.textContent = message;
      feedback.classList.toggle("text-danger", isError);
      feedback.classList.toggle("text-muted", !isError);
    };

    const scan = () => {
      const code = input.value.trim().toLowerCase();
      if (!code) return;

      const form = input.closest("form") || document;
      const targetSelector = input.dataset.scanTarget || "[data-adjustment-table]";
      const table = form.querySelector(targetSelector);
      if (!table) return;

      const productSelects = Array.from(table.querySelectorAll("[data-product-select]"));
      let matchedValue = "";
      let matchedText = "";

      for (const select of productSelects) {
        const option = Array.from(select.options).find((item) =>
          item.value &&
          [item.dataset.barcode, item.dataset.sku, item.dataset.code].some((value) => (value || "").trim().toLowerCase() === code));
        if (option) {
          matchedValue = option.value;
          matchedText = option.textContent?.trim() || "product";
          break;
        }
      }

      if (!matchedValue) {
        setFeedback(`No product found for "${input.value.trim()}".`, true);
        input.select();
        return;
      }

      const targetSelect =
        productSelects.find((select) => select.value === matchedValue) ||
        productSelects.find((select) => !select.value) ||
        productSelects[0];

      if (!targetSelect) return;
      const wasAlreadySelected = targetSelect.value === matchedValue;
      targetSelect.value = matchedValue;
      targetSelect.dispatchEvent(new Event("change", { bubbles: true }));

      const row = targetSelect.closest("tr");
      const qtyInput = row?.querySelector("[data-adjustment-qty]");
      if (qtyInput) {
        const currentQty = parseNumber(qtyInput.value);
        qtyInput.value = wasAlreadySelected ? String(currentQty + 1) : (qtyInput.value || "1");
      }
      const countedInput = row?.querySelector("[data-counted-qty]");
      if (countedInput) {
        const currentCounted = parseNumber(countedInput.value);
        countedInput.value = String(currentCounted + 1);
      }
      row?.dispatchEvent(new Event("inventory:recalculate", { bubbles: true }));

      setFeedback(`Added ${matchedText}.`);
      input.value = "";
      input.focus();
    };

    input.addEventListener("keydown", (event) => {
      if (event.key !== "Enter") return;
      event.preventDefault();
      scan();
    });
  });

  let scanBuffer = "";
  let lastScanKeyAt = 0;

  document.addEventListener("keydown", (event) => {
    const target = event.target;
    const isTypingTarget = target instanceof HTMLElement &&
      (target.matches("input, textarea, select") || target.isContentEditable);
    if (isTypingTarget) return;

    const now = Date.now();
    if (now - lastScanKeyAt > 120) scanBuffer = "";
    lastScanKeyAt = now;

    if (event.key === "Enter") {
      const scannedValue = scanBuffer.trim();
      scanBuffer = "";
      if (scannedValue.length < 3) return;

      const searchForm = document.querySelector("[data-scan-search-form]");
      const searchInput = searchForm?.querySelector("[data-scan-search-input]");
      if (searchForm && searchInput) {
        event.preventDefault();
        const modalParam = searchForm.dataset.scanModalParam;
        const modalInput = modalParam
          ? searchForm.querySelector(`[name="${CSS.escape(modalParam)}"], [data-scan-modal-input]`)
          : null;
        if (modalInput) {
          modalInput.value = scannedValue;
          searchInput.value = "";
        } else {
          searchInput.value = scannedValue;
        }
        searchForm.submit();
        return;
      }

      const opener = document.querySelector("[data-auto-open-scan-modal]");
      const modalSelector = opener?.dataset.autoOpenScanModal;
      const inputSelector = opener?.dataset.autoOpenScanInput;
      const scanInput = inputSelector ? document.querySelector(inputSelector) : null;
      const modalElement = modalSelector ? document.querySelector(modalSelector) : null;
      if (!modalElement || !scanInput || !window.bootstrap) return;

      event.preventDefault();
      const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
      modalElement.addEventListener("shown.bs.modal", () => {
        scanInput.value = scannedValue;
        scanInput.focus();
        scanInput.dispatchEvent(new KeyboardEvent("keydown", { key: "Enter", bubbles: true }));
      }, { once: true });
      modal.show();
      return;
    }

    if (event.key.length === 1) {
      scanBuffer += event.key;
    }
  });
});
