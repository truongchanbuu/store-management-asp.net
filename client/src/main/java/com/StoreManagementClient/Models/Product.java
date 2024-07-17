package com.StoreManagementClient.Models;

import lombok.*;

import java.time.LocalDateTime;

@Data
@Getter
@Setter
@AllArgsConstructor
@NoArgsConstructor
@ToString
public class Product {
    private String pid;
    private String name;
    private Category category;
    private double importPrice;
    private double retailPrice;
    private String barcode;
    private String illustrator;
    private int quantity;
    private LocalDateTime createdAt;
    private LocalDateTime updatedAt;
}
