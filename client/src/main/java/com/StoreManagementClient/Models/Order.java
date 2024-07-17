package com.StoreManagementClient.Models;

import lombok.Data;
import lombok.Getter;
import lombok.Setter;

import java.time.LocalDateTime;
import java.util.List;

@Data
@Getter
@Setter
public class Order {
    private String oid;
    private Customer customer;
    private User user;
    private double totalPrice;
    private Status orderStatus;
    private List<OrderProduct> orderProducts;
    private LocalDateTime createdAt;
    private LocalDateTime updatedAt;
}
