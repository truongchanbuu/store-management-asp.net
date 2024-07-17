const baseURL = "http://localhost:8080/api";

function getCookie(name) {
    const cookieValue = document.cookie
        .split('; ')
        .find(row => row.startsWith(name + '='))
        .split('=')[1];

    return cookieValue ? decodeURIComponent(cookieValue) : null;
}

const token = getCookie('token');

if (!token) {
    window.location.href = "/auth/login";
}

async function getProductByPID(pid) {
    try {
        let response = await axios.get(`${baseURL}/transactions/product/${pid}`);

        return response.data.data[0];
    } catch (error) {
        console.log(error);
    }
}

async function getTotalUser() {
    try {
        let response = await axios.get(`${baseURL}/users/total`);

        return response.data.data[0];
    } catch (error) {
        console.log(error);
    }
}

async function getTotalProduct() {
    try {
        let response = await axios.get(`${baseURL}/product/total`);

        return response.data.data[0];
    } catch (error) {
        console.log(error);
    }
}


async function getTotalCustomer() {
    try {
        let response = await axios.get(`${baseURL}/customer/total`);

        return response.data.data[0];
    } catch (error) {
        console.log(error);
    }
}

async function getTotalOrder() {
    try {
        let response = await axios.get(`${baseURL}/order/total`);

        return response.data.data[0];
    } catch (error) {
        console.log(error);
    }
}

async function getAllOrders() {
    try {
        let response = await axios.get(`${baseURL}/order`);

        return response.data.data;
    } catch (error) {
        console.log(error);
    }
}

async function getOrdersByTimeline(timeline, startDate, endDate) {
    try {
        let url = `${baseURL}/reports/sale-results?timeline=${timeline}`;
        if (startDate && endDate) {
            url += `&startDate=${startDate}&endDate=${endDate}`;
        }

        const response = await axios.get(url);

        return response.data.data[0]?.orders || [];
    } catch (error) {
        console.log(error);
    }
}

async function fetchTables(orders) {
    const allOrderProducts = orders.flatMap(order => order.orderStatus === 'COMPLETED' ? order.orderProducts : []);
    const productQuantityMap = new Map();

    allOrderProducts.forEach(orderProduct => {
        const productId = orderProduct.pid;
        const quantity = orderProduct.quantity;

        if (productQuantityMap.has(productId)) {
            productQuantityMap.set(productId, productQuantityMap.get(productId) + quantity);
        } else {
            productQuantityMap.set(productId, quantity);
        }
    });

    const productQuantityArray = Array.from(productQuantityMap, ([productId, quantity]) => ({ productId, quantity }));
    productQuantityArray.sort((a, b) => b.quantity - a.quantity);

    const top5Products = productQuantityArray.slice(0, 5);

    const productDetailsPromises = top5Products.map(async product => {
        const productDetails = await getProductByPID(product.productId);
        return {
            name: productDetails ? productDetails.name : 'N/A',
            amountSold: product.quantity
        };
    });

    const topProductDetails = await Promise.all(productDetailsPromises);

    const topProductTableBody = $('#top-product-table tbody');
    topProductTableBody.empty();

    topProductDetails.forEach((product, index) => {
        const newRow = `
                <tr>
                    <td>${index + 1}</td>
                    <td>${product.name}</td>
                    <td>${product.amountSold}</td>
                </tr>
            `;
        topProductTableBody.append(newRow);
    });

    const userRevenueMap = new Map();

    orders.forEach(order => {
        const userName = order.user.username;
        const userEmail = order.user.email;
        const totalPrice = parseFloat(order.totalPrice);

        if (userRevenueMap.has(userEmail)) {
            const existingEntry = userRevenueMap.get(userEmail);
            existingEntry.totalPrice += totalPrice;
            existingEntry.userName = userName;

            userRevenueMap.set(userEmail, existingEntry);
        } else {
            userRevenueMap.set(userEmail, {
                userName,
                userEmail,
                totalPrice
            });
        }
    });

    const userRevenueArray = Array.from(userRevenueMap.values());

    userRevenueArray.sort((a, b) => b.totalPrice - a.totalPrice);

    const top5Employees = userRevenueArray.slice(0, 5);

    const topEmployeeTableBody = $('#top-employee-table tbody');
    topEmployeeTableBody.empty();

    top5Employees.forEach((employee, index) => {
        const newRow = `
                <tr>
                    <td>${index + 1}</td>
                    <td>${employee.userName}</td>
                    <td>${employee.userEmail}</td>
                    <td>${employee.totalPrice.toFixed(2)}</td>
                </tr>
            `;
        topEmployeeTableBody.append(newRow);
    });
}

$(document).ready(async () => {
    let totalUser = await getTotalUser();
    let totalProduct = await getTotalProduct();
    let totalCustomer = await getTotalCustomer();
    let totalOrder = await getTotalOrder();

    const orders = await getAllOrders();
    const totalSales = orders.reduce((total, order) => total + (order.totalPrice || 0), 0);

    $('#total-users').html(totalUser);
    $('#total-products').html(totalProduct);
    $('#total-customers').html(totalCustomer);
    $('#total-orders').html(totalOrder);
    $('#total-sales').html(`$${totalSales.toFixed(2)}`);

    let ordersByTime = await getOrdersByTimeline('today');
    fetchTables(ordersByTime);

    $('#date-range-select').on('change', async function () {
        let selectedTimeline = $(this).val();

        if (selectedTimeline === 'custom') {
            $('#customDateRange').removeClass('d-none');
        } else {
            $('#customDateRange').addClass('d-none');
            ordersByTime = await getOrdersByTimeline(selectedTimeline);
            fetchTables(ordersByTime);
        }
    });

    $('#search-custom-date').on('click', async function () {
        const startDate = new Date($('#startDate').val() + 'T00:00:00Z').toISOString();
        const endDate = new Date($('#endDate').val() + 'T23:59:59Z').toISOString();

        ordersByTime = await getOrdersByTimeline('custom', startDate, endDate);
        fetchTables(ordersByTime);
    });
});
