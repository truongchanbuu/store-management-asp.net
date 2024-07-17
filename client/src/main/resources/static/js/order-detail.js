const baseURL = "http://localhost:8080/api";
const urlParts = window.location.pathname.split('/');
const oid = urlParts[urlParts.length - 1];

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


const getOrderByOid = async oid => {
    try {
        const response = await axios.get(`${baseURL}/transactions/orders/${oid}`);

        if (response.status === 200 && response.data.data && response.data.data.length > 0) {
            return response.data.data[0];
        } else {
            console.log("Failed to fetch order or order not found");
            return null;
        }
    } catch (error) {
        console.error("Error fetching order:", error);
        return null;
    }
};

const getProductByPid = async pid => {
    try {
        const response = await axios.get(`${baseURL}/transactions/product/${pid}`);
        return response.data.data[0];
    } catch (error) {
        console.error("Error fetching product details:", error);
        return null;
    }
};

const placeOrder = async (oid) => {
    try {
        const response = await axios.get(`${baseURL}/transactions/orders/${oid}/cash/success`);

        if (response.status === 200) {
            return response.data.message;
        } else {
            console.error('Failed to place order:', response.status);
            return null;
        }
    } catch (error) {
        console.error('Error placing order:', error);
        return null;
    }
};

let currentOrder;
(async () => {
    currentOrder = await getOrderByOid(oid);

    if (currentOrder) {
        const options = {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: 'numeric',
            minute: 'numeric',
            second: 'numeric',
            timeZone: 'UTC'
        };

        document.getElementById("oidValue").textContent = currentOrder.oid;
        document.getElementById("customerNameValue").textContent = currentOrder.customer.name;
        document.getElementById("userNameValue").textContent = currentOrder.user.username;
        document.getElementById("dateCreatedValue").textContent = new Date(currentOrder.createdAt).toLocaleString('en-US', options);

        const productTableBody = document.getElementById("product-table-body");
        currentOrder.orderProducts.forEach(async product => {
            const productDetails = await getProductByPid(product.pid);
            console.log(productDetails);

            const row = document.createElement("tr");

            const productCell = document.createElement("td");
            const productImage = document.createElement("img");
            productImage.src = productDetails.illustrator;
            productImage.alt = "Product Image";
            productImage.height = 52;
            const productName = document.createElement("p");
            productName.className = "d-inline-block align-middle mb-0 product-name f_s_16 f_w_600 color_theme2";
            productName.textContent = productDetails.name;
            productCell.appendChild(productImage);
            productCell.appendChild(productName);

            const quantityCell = document.createElement("td");
            quantityCell.textContent = product.quantity;

            const totalCell = document.createElement("td");
            totalCell.textContent = formatCurrency(product.quantity * product.retailPrice);

            row.appendChild(productCell);
            row.appendChild(quantityCell);
            row.appendChild(totalCell);

            productTableBody.appendChild(row);
        });

        document.getElementById("total-payment").textContent = formatCurrency(currentOrder.totalPrice);

    } else {
        console.log("Failed to fetch current order or order not found");
        window.location.href = '/error';
    }
})();

const formatCurrency = amount => `$${amount.toFixed(2)}`;

$("#cash-received").on("input", function() {
    let totalPrice = parseFloat($('#total-payment').text().replace('$', ''));
    let cashReceived = parseFloat($(this).val());
    
    $('#cash-return').val((cashReceived - totalPrice).toFixed(2));
});

$('#place-order').on('click', async () => {
    try {
        const cashReceived = parseFloat($('#cash-received').val());
        const totalPrice = parseFloat($('#total-payment').text().replace('$', ''));

        if (isNaN(cashReceived) || cashReceived < totalPrice) {
            alert('Cash received must be equal to or greater than the total amount.');
            return;
        }

        $('#place-order').prop('disabled', true);

        console.log("PLACE ORDER");
        const orderPlaced = await placeOrder(oid);

        if (orderPlaced) {
            const options = {
                year: 'numeric',
                month: 'long',
                day: 'numeric',
                hour: 'numeric',
                minute: 'numeric',
                second: 'numeric',
                timeZone: 'UTC'
            };

            alert(orderPlaced);
            localStorage.clear();
            sessionStorage.clear();
            window.jsPDF = window.jspdf.jsPDF;

            var pdf = new window.jsPDF();

            pdf.text(20, 20, `Order ID: ${currentOrder.oid}`);
            pdf.text(20, 30, `Customer Name: ${currentOrder.customer.name}`);
            pdf.text(20, 40, `User Name: ${currentOrder.user.username}`);
            pdf.text(20, 50, `Date Created: ${new Date(currentOrder.createdAt).toLocaleString('en-US', options)}`);

            let yPos = 60;
            for (const product of currentOrder.orderProducts) {
                const productDetails = await getProductByPid(product.pid);
                pdf.text(20, yPos, `Product Name: ${productDetails.name}`);
                pdf.text(20, yPos + 10, `Quantity: ${product.quantity}`);
                pdf.text(20, yPos + 20, `Total: ${formatCurrency(product.quantity * product.retailPrice)}`);
                yPos += 30;
            }        

            pdf.text(20, yPos, `Total Payment: ${formatCurrency(currentOrder.totalPrice)}`);
            pdf.text(20, yPos + 10, `Cash Received: ${$('#cash-received').val()}`);
            pdf.text(20, yPos + 20, `Cash Return: ${$('#cash-return').val()}`);

            pdf.save('order_details.pdf');

            window.location.href = `/Home`;
        } else {
            alert("FAILED");
        }

    } catch (error) {
        console.error('Error placing order:', error);
    } finally {
        $('#place-order').prop('disabled', false);
    }
});