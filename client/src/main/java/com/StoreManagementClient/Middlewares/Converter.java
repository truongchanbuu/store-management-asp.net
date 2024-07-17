package com.StoreManagementClient.Middlewares;

import com.StoreManagementClient.Models.*;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

public class Converter<T> {

    private static final DateTimeFormatter formatter = DateTimeFormatter.ISO_OFFSET_DATE_TIME;

    public static List<User> convertToUsers(List<Map<String, Object>> usersMap) {
        if (usersMap == null) return null;

        List<User> users = new ArrayList<User>();
        for (Map<String, Object> userMap : usersMap) {
            User user = convertToUser(userMap);
            if (user != null) users.add(user);
        }

        return users;
    }

    public static User convertToUser(Map<String, Object> userMap) {
        if (userMap == null) return null;

        User user = new User();
        user.setId((String) userMap.get("id"));
        user.setEmail((String) userMap.get("email"));
        user.setUsername((String) userMap.get("username"));
        user.setPassword((String) userMap.get("password"));
        user.setStatus(Status.valueOf((String) userMap.get("status")));
        user.setRole(Role.valueOf((String) userMap.get("role")));
        user.setAvatar((String) userMap.get("avatar"));

        return user;
    }

    public static List<Customer> convertToCustomers(List<Map<String, Object>> customersMap) {
        if (customersMap == null) return null;

        List<Customer> customers = new ArrayList<>();
        for (Map<String, Object> customerMap : customersMap) {
            Customer customer = convertToCustomer(customerMap);
            if (customer != null) customers.add(customer);
        }

        return customers;
    }

    public static Customer convertToCustomer(Map<String, Object> customerMap) {
        if (customerMap == null) return null;

        Customer customer = new Customer();
        customer.setCustId((String) customerMap.get("custId"));
        customer.setName((String) customerMap.get("name"));
        customer.setPhone((String) customerMap.get("phone"));
        customer.setEmail((String) customerMap.get("email"));
        customer.setPoint(((Number) customerMap.get("point")).doubleValue());

        return customer;
    }

    public static List<Order> convertToOrders(List<Map<String, Object>> data) {
        if (data == null) return null;

        List<Order> orders = new ArrayList<>();
        for (Map<String, Object> orderMap : data) {
            Order order = convertToOrder(orderMap);
            if (order != null) orders.add(order);
        }

        return orders;
    }

    public static Order convertToOrder(Map<String, Object> orderMap) {
        if (orderMap == null) return null;

        Order order = new Order();
        order.setOid((String) orderMap.get("oid"));
        order.setCustomer(convertToCustomer((Map<String, Object>) orderMap.get("customer")));
        order.setUser(convertToUser((Map<String, Object>) orderMap.get("user")));
        order.setTotalPrice(((Number) orderMap.get("totalPrice")).doubleValue());
        order.setOrderStatus(Status.valueOf((String) orderMap.get("orderStatus")));

        String createdAtString = (String) orderMap.get("createdAt");
        order.setCreatedAt(createdAtString != null ?
                parseLocalDateTime(createdAtString, formatter) : null);

        String updatedAtString = (String) orderMap.get("updatedAt");
        order.setUpdatedAt(updatedAtString != null ?
                parseLocalDateTime(updatedAtString, formatter) : null);

        List<OrderProduct> orderProducts = convertToOrderProducts((List<Map<String, Object>>) orderMap.get("orderProducts"));
        order.setOrderProducts(orderProducts);

        return order;
    }

    public static List<OrderProduct> convertToOrderProducts(List<Map<String, Object>> orderProductsMap) {
        if (orderProductsMap == null) return null;

        List<OrderProduct> orderProducts = new ArrayList<>();
        for (Map<String, Object> orderProductMap : orderProductsMap) {
            OrderProduct orderProduct = convertToOrderProduct(orderProductMap);
            if (orderProduct != null) orderProducts.add(orderProduct);
        }

        return orderProducts;
    }

    public static OrderProduct convertToOrderProduct(Map<String, Object> orderProductMap) {
        if (orderProductMap == null) return null;

        OrderProduct orderProduct = new OrderProduct();
        orderProduct.setId((String) orderProductMap.get("id"));
        orderProduct.setPid((String) orderProductMap.get("pid"));
        orderProduct.setOid((String) orderProductMap.get("oid"));
        orderProduct.setQuantity((Integer) orderProductMap.get("quantity"));
        orderProduct.setRetailPrice(((Number) orderProductMap.get("retailPrice")).doubleValue());
        orderProduct.setImportPrice(((Number) orderProductMap.get("importPrice")).doubleValue());

        return orderProduct;
    }

    public static List<Product> convertToProducts(List<Map<String, Object>> productsMap) {
        if (productsMap == null) return null;

        List<Product> products = new ArrayList<>();
        for (Map<String, Object> productMap : productsMap) {
            Product product = convertToProduct(productMap);
            if (product != null) products.add(product);
        }

        return products;
    }

    public static Product convertToProduct(Map<String, Object> productMap) {
        if (productMap == null) return null;

        Product product = new Product();
        product.setPid((String) productMap.get("pid"));
        product.setName((String) productMap.get("name"));
        product.setCategory(Category.valueOf((String) productMap.get("category")));
        product.setImportPrice(((Number) productMap.get("importPrice")).doubleValue());
        product.setRetailPrice(((Number) productMap.get("retailPrice")).doubleValue());
        product.setBarcode((String) productMap.get("barcode"));
        product.setIllustrator((String) productMap.get("illustrator"));
        product.setQuantity((Integer) productMap.get("quantity"));

        String createdAtString = (String) productMap.get("createdAt");
        product.setCreatedAt(createdAtString != null ?
                parseLocalDateTime(createdAtString, formatter) : null);

        String updatedAtString = (String) productMap.get("updatedAt");
        product.setUpdatedAt(updatedAtString != null ?
                parseLocalDateTime(updatedAtString, formatter) : null);

        return product;
    }

    private static LocalDateTime parseLocalDateTime(String dateTimeString, DateTimeFormatter... formatters) {
        for (DateTimeFormatter formatter : formatters) {
            try {
                return LocalDateTime.parse(dateTimeString, formatter);
            } catch (Exception ignored) {
                // Continue trying with the next formatter
            }
        }
        return null; // Return null if none of the formatters succeed
    }

}
