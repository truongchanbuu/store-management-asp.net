package com.StoreManagementClient.Models;

import lombok.Data;

@Data
public class OrderProduct {
	private String id;
	private String pid;
	private String oid;
	private int quantity;
	private double importPrice;
	private double retailPrice;
}
