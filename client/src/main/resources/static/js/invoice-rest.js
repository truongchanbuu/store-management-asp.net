const baseURL = "http://localhost:8080/api";
let oid = sessionStorage.getItem("currentOID");

function getCookie(name) {
    const cookieValue = document.cookie
        .split('; ')
        .find(row => row.startsWith(name + '='))
        .split('=')[1];

    return cookieValue ? decodeURIComponent(cookieValue) : null;
}

async function getUserByEmail(email) {
    try {
        const response = await axios.get(`${baseURL}/admin/users`, {
            params: {
                email: email
            }
        });

        if (response.status === 200) {
            const users = response.data.users;

            if (users && users.length > 0) {
                return users[0];
            } else {
                console.log("User not found.");
                return null;
            }
        } else {
            console.log(`Request failed with status ${response.status}`);
            return null;
        }
    } catch (error) {
        console.error('Error fetching user:', error);
        return null;
    }
}

const token = getCookie('token');

if (!token) {
    window.location.href = "/auth/login";
}

const [header, payload, signature] = token.split('.');
const decodedPayload = atob(payload.replace(/_/g, '/').replace(/-/g, '+'));
const claims = JSON.parse(decodedPayload);
const email = claims.sub;

if (!oid) {
    $(document).ready(() => {
        createOrder(email);
    });
}

function displaySuccessAlert(message) {
    let alertContainer = $("#alert-container");

    let alertHTML = `
        <div class="alert alert-success alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>
        `;

    let $alert = $(alertHTML);
    alertContainer.append($alert);

    $alert.on("closed.bs.alert", () => {
        $("#open-modal-customer-button").click();
    });

    setTimeout(() => {
        $alert.alert("close");
    }, 1000);
}

function displayDangerAlert(message) {
    let alertContainer = $("#alert-container");
    let alertHTML = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="close" data-dismiss="alert" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>
    `;

    alertContainer.append(alertHTML);
    $(".alert").alert();
    setTimeout(() => {
        $(".alert").alert("close");
    }, 2000);
}

// Search data
const debounce = (func, delay) => {
    let timeoutId;
    return function () {
        const context = this;
        const args = arguments;
        clearTimeout(timeoutId);
        timeoutId = setTimeout(() => func.apply(context, args), delay);
    };
};

// Create order with status PENDING
async function createOrder(email) {
    try {
        const user = await getUserByEmail(email);
        console.log(user)
        let response = await axios.post(
            `${baseURL}/transactions/create`,
            user,
            {
                headers: {
                    "Content-Type": "application/json",
                },
            }
        );

        console.log("Order created successfully:", response.data);
        oid = response.data.data[0].oid;
        sessionStorage.setItem("currentOID", oid);
        sessionStorage.setItem(
            "currentOrder",
            JSON.stringify(response.data.data[0])
        );

        $(".card.QA_table").attr("data-id", oid);
    } catch (error) {
        console.error("Error creating order:", error);
        window.location.href = "/error";
    }
}

const handleSearchAndUpdateList = async (text) => {
    try {
        // API can be changed later...
        let response = await axios.get(
            `${baseURL}/transactions/search-name?productName=${text}`
        );

        let products = response.data.data;
        
        if (products == null || products.length === 0) {
            // API can be changed later...
            response = await axios.get(
                `${baseURL}/transactions/search-barcode?barcode=${text}`
            );
            products = response.data.data;
        }

        displaySearchResults(products);
    } catch (e) {
        console.log(e);
    }
};

const changeOrderStatusFailed = async () => {
    try {
        let response = await axios.post(
            `${baseURL}/transactions/orders/${oid}/update-status`,
            null,
            {
                params: {
                    status: "FAILED",
                },
                headers: { "Content-Type": "application/json" },
            }
        );

        const updatedOrder = response.data.data[0];

        console.log("Order status changed successfully:", updatedOrder);
        window.location.href = "/Home";
    } catch (e) {
        console.log(e);
    }
};

const displaySearchResults = (results) => {
    results.forEach((result) => {
        const resultDiv = document.createElement("div");
        resultDiv.setAttribute("data-id", result.pid);
        resultDiv.classList.add(
            "list-group-item-action",
            "list-group-item-light",
            "pt-3"
        );
        resultDiv.innerHTML = `
                                <div class="product-item">
                                    <img src="${result.illustrator}" alt="Product Image" class="product-image">
                                    <div class="product-details">
                                        <h3 class="product-name">${result.name}</h3>
                                        <p class="product-price">$${result.retailPrice}</p>
                                    </div>
                                </div>
                            `;
        searchResultsContainer.appendChild(resultDiv);
    });
};
// End Search Data

async function fetchProductByIdAndAddToLocalStorage(id) {
    try {
        // API can be changed later...
        let response = await axios.get(`${baseURL}/transactions/product/${id}`);

        let product = response.data.data[0];

        let orderedProduct = {
            pid: id,
            oid,
            quantity: 1,
            importPrice: product.importPrice,
            retailPrice: product.retailPrice,
        };

        let productsInLocalStorage =
            JSON.parse(localStorage.getItem(`products-${oid}`)) || [];
        let isProductExist = productsInLocalStorage.some(
            (item) => item.pid === orderedProduct.pid
        );

        if (!isProductExist) {
            if (orderedProduct.quantity <= product.quantity) {
                productsInLocalStorage.push(orderedProduct);
            } else {
                alert(`Exceeded maximum quantity limit (${product.quantity})`);
            }
        } else {
            const existingProduct = productsInLocalStorage.find(
                item => item.pid === id
            );

            if (existingProduct.quantity + 1 <= product.quantity) {
                existingProduct.quantity += 1;
            } else {
                alert(`Exceeded maximum quantity limit (${product.quantity})`);
            }
        }

        localStorage.setItem(
            `products-${oid}`,
            JSON.stringify(productsInLocalStorage)
        );

        updateTableFromLocalStorage(oid);

        searchInput.value = "";
        searchResultsContainer.innerHTML = "";
    } catch (error) {
        console.error("Error fetching product:", error);
    }
}

async function updateTableFromLocalStorage(oid) {
    const tableBody = document.querySelector(".table tbody");
    tableBody.innerHTML = "";

    const productsInLocalStorage =
        JSON.parse(localStorage.getItem(`products-${oid}`)) || [];

    let total = 0;

    await Promise.all(
        productsInLocalStorage.map(async (prd, index) => {
            // API can be changed later...
            let response = await axios.get(
                `http://localhost:8080/api/transactions/product/${prd.pid}`
            );

            let product = response.data.data[0];

            const row = document.createElement("tr");
            row.setAttribute("data-id", prd.pid);

            const productTotal =
                (product.retailPrice || 0) * (prd.quantity || 0);
            total += productTotal;

            row.innerHTML = `
            <td class="left strong">${product.name}</td>
            <td class="right">$${(product.retailPrice || 0).toFixed(2)}</td>
            <td class="center">
                <div class="input-group quantity-control">
                    <button id="minus-${
                        prd.pid
                    }-${oid}" class="quantity-btn btn btn-secondary" type="button">-</button>
                    <span class="quantity input-group-text">${
                        prd.quantity || 0
                    }</span>
                    <button id="plus-${
                        prd.pid
                    }-${oid}" class="quantity-btn btn btn-secondary" type="button">+</button>
                </div>
            </td>
            <td class="right item-price-${prd.pid}">$${productTotal.toFixed(
                2
            )}</td>
        `;

            tableBody.appendChild(row);

            const minusBtn = document.getElementById(`minus-${prd.pid}-${oid}`);
            const plusBtn = document.getElementById(`plus-${prd.pid}-${oid}`);

            minusBtn.addEventListener("click", () =>
                adjustQuantity(product, oid, -1)
            );
            plusBtn.addEventListener("click", () =>
                adjustQuantity(product, oid, 1)
            );
        })
    );

    wholeTotal.innerHTML = `$${total.toFixed(2)}`;
}

async function adjustQuantity(product, orderId, adjustment) {
    let productsInLocalStorage =
        JSON.parse(localStorage.getItem(`products-${orderId}`)) || [];

    const existingProduct = productsInLocalStorage.find(
        (item) => item.pid === product.pid && item.oid === orderId
    );

    const oldQuantity = existingProduct.quantity;
    existingProduct.quantity = Math.max(
        0,
        existingProduct.quantity + adjustment
    );

    if (existingProduct.quantity > product.quantity) {
        existingProduct.quantity = oldQuantity;

        alert("Exceed the limit quantity!");
        return;
    }

    if (existingProduct.quantity === 0) {
        const deleteConfirm = await confirmDelete();
        if (deleteConfirm) {
            productsInLocalStorage = productsInLocalStorage.filter(
                item => item.quantity !== 0
            );

            // await deleteOrderProduct(existingProduct.oid, existingProduct.pid);
        }
    }

    let total = existingProduct.quantity * product.retailPrice;

    $(`.item-price-${product.pid}`).html(`$${total.toFixed(2)}`);

    localStorage.setItem(
        `products-${orderId}`,
        JSON.stringify(productsInLocalStorage)
    );
    updateTableFromLocalStorage(orderId);
}

// async function deleteOrderProduct(oid, pid) {
//     try {
//         const response = await axios.delete(`${baseURL}/transactions/order-products?oid=${oid}&pid=${pid}`);

//         if (response.status === 200) {
//             console.log("OrderProduct deleted successfully");
//         } else {
//             console.error("Failed to delete OrderProduct");
//         }
//     } catch (error) {
//         console.error("Error deleting OrderProduct", error);
//     }
// }

function confirmDelete() {
    return new Promise((resolve) => {
        $("#remove-modal").modal("show");

        $(".btn-confirm-delete").on("click", () => {
            $("#remove-modal").modal("hide");
            resolve(true);
        });
    });
}

async function getCustomerByPhone(phone) {
    try {
        let response = await axios.get(`${baseURL}/customer?phone=${phone}`);

        return response.data.data;
    } catch (error) {
        console.log("Fetching error: " + error);
        window.location.href = "/error";
    }
}

const createCustomer = async (name, phone, email) => {
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    const phonePattern = /^\d{10}$/;

    if (
        !name ||
        !phone ||
        !email ||
        !emailPattern.test(email) ||
        !phonePattern.test(phone)
    ) {
        displayDangerAlert("Please enter valid information");
        return null;
    }

    try {
        let customer = {
            name,
            phone,
            email,
        };

        let response = await axios.post(
            `${baseURL}/customer/create`,
            customer,
            {
                headers: {
                    "Content-Type": "application/json",
                },
            }
        );

        return response.data.data[0];
    } catch (e) {
        console.log(e);
        return null;
    }
};

const getAllOrderProducts = async (oid) => {
    try {
        let response = await axios.get(
            `${baseURL}/transactions/order-products/${oid}`
        );

        if (
            response.status === 200 &&
            response?.data.data &&
            response.data.data.length > 0
        ) {
            return response.data.data;
        } else {
            console.log("There is nothing");
            return null;
        }
    } catch (e) {
        console.log(e);
        return null;
    }
};

async function saveAllOrderProduct(orderProducts) {
    try {
        const response = await axios.post(
            `${baseURL}/transactions/order-products/create`,
            orderProducts
        );

        if (response.status === 200) {
            console.log("Order products saved successfully:", response.data);
            return response.data.data;
        } else {
            console.error("Failed to save order products:", response.data);
            return null;
        }
    } catch (error) {
        console.error("Error while saving order products:", error.message);
        return null;
    }
}

const acceptOrder = async (order) => {
    try {
        const response = await axios.post(
            `${baseURL}/transactions/orders/${oid}`,
            order,
            {
                headers: {
                    "Content-Type": "application/json",
                },
            }
        );

        if (
            response.status === 200 &&
            response.data &&
            response.data.data &&
            response.data.data.length > 0
        ) {
            return response.data.data[0];
        } else {
            console.error("Invalid response format:", response);
            return null;
        }
    } catch (error) {
        console.error("Error accepting order:", error);
        return null;
    }
};

let currentDateTag = document.querySelector(".current-date");
const currentDate = new Date().toLocaleDateString("en-GB");
currentDateTag.innerHTML = currentDate;

const searchInput = document.getElementById("search-product-by-name");
const searchResultsContainer = document.getElementById("searchlist");
const wholeTotal = document.querySelector(".whole-total");

updateTableFromLocalStorage(oid);

searchInput.addEventListener(
    "input",
    debounce(() => {
        searchResultsContainer.innerHTML = "";
        const searchTerm = searchInput.value.trim();

        if (searchTerm !== "") {
            handleSearchAndUpdateList(searchTerm);
        }
    }, 300)
);

$("#searchlist").on("click", ".list-group-item-action", function () {
    const productId = $(this).data("id");

    fetchProductByIdAndAddToLocalStorage(productId);
});

$(".modal-confirm-payment").on("click", () => {
    let productsInLocalStorage = JSON.parse(
        localStorage.getItem(`products-${oid}`)
    );

    if (productsInLocalStorage === null) {
        $("#modal-customer").modal("dispose");
        alert("Please add product or cancel the order");
        return;
    } else {
        let isExistedOID = productsInLocalStorage.findIndex(
            (item) => item.oid === oid
        );

        if (isExistedOID === -1) {
            $("#modal-customer").modal("dispose");
            alert("Please add product or cancel the order");
        }
    }
});

$(".confirm-payment").on("click", async () => {
    let phone = $("#customer-phone-number").val();

    var phoneno = /^\d{10}$/;
    if (!phone.match(phoneno)) {
        alert("Invalid phone number");
        return;
    }

    let customer = await getCustomerByPhone(phone);

    if (customer.length === 0) {
        return alert("Invalid customer");
    }

    currentOrder = JSON.parse(sessionStorage.getItem("currentOrder"));

    let orderProducts = await saveAllOrderProduct(
        JSON.parse(localStorage.getItem(`products-${oid}`))
    );

    if (orderProducts !== null || orderProducts?.length === 0) {
        currentOrder.oid = oid;
        currentOrder.customer = customer[0];
        currentOrder.orderProducts = orderProducts;
        
        let order = await acceptOrder(currentOrder);
        
        if (order) {
            window.location.href = `/orders/${oid}`;
        } else {
            alert("FAILED TO ADD PRODUCTS TO ORDER");
        }
    }
});

$(".btn-create-customer").on("click", async () => {
    let name = $("#customer-name").val();
    let phone = $("#phone").val();
    let email = $("#customer-email").val();

    let customer = await createCustomer(name, phone, email);

    if (!customer) {
        displayDangerAlert("Failed to create customer");
    } else {
        displaySuccessAlert("Customer created successfully");
    }
});

$(".btn-cancel-order").on("click", async () => {
    $("#remove-modal .modal-body").html(
        "Do you want to delete this order? This cannot be undone"
    );

    $("#remove-modal").modal("show");

    let deleteConfirm = await confirmDelete();
    if (deleteConfirm) {
        changeOrderStatusFailed();
    }
});

$('.btn-create-new-order').on('click', () => {
    const newTab = window.open("/orders", '_blank');
    newTab.sessionStorage.clear();
})
