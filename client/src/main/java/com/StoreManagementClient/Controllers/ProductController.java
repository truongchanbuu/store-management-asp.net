package com.StoreManagementClient.Controllers;

import com.StoreManagementClient.Models.Category;
import com.StoreManagementClient.Models.Product;
import com.StoreManagementClient.Models.User;
import com.StoreManagementClient.Services.ProductService;
import jakarta.servlet.http.HttpServletRequest;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.servlet.mvc.support.RedirectAttributes;

import java.util.List;

@Controller
@RequestMapping("")
public class ProductController {
    private final ProductService productService;

    @Autowired
    public ProductController(ProductService productService) {
        this.productService = productService;
    }

    @GetMapping("/products")
    public String getAllProducts(@RequestParam(required = false) String text, Model model, HttpServletRequest request) {
        User user = (User) request.getAttribute("authenticatedUser");
        model.addAttribute("user", user);

        List<Product> products = productService.getProducts(text);
        model.addAttribute("products", products);
        return "Products/product";
    }

    @PostMapping("/admin/products/create")
    public String createProduct(@RequestParam String name,
                                @RequestParam Category category,
                                @RequestParam Object quantity,
                                @RequestParam Object importPrice,
                                @RequestParam Object retailPrice,
                                @RequestParam String barcode,
                                @RequestParam String illustrator,
                                RedirectAttributes redirectAttrs,
                                HttpServletRequest request) {
        if (name.isEmpty() || barcode.isEmpty() || illustrator.isEmpty() || importPrice == null || retailPrice == null || quantity == null) {
            redirectAttrs.addFlashAttribute("error", "Please fill in all fields");
            return "redirect:/products";
        }

        try {
            Integer.parseInt(quantity.toString());
        } catch (NumberFormatException e) {
            redirectAttrs.addFlashAttribute("error", "Quantity must be a number");
            return "redirect:/products";
        }

        try {
            Double.parseDouble(importPrice.toString());
            Double.parseDouble(retailPrice.toString());
        } catch (NumberFormatException e) {
            redirectAttrs.addFlashAttribute("error", "Price must be a number");
            return "redirect:/products";
        }

        Product product = new Product(null, name, category,
                Double.parseDouble(importPrice.toString()),
                Double.parseDouble(retailPrice.toString()),
                barcode, illustrator,
                Integer.parseInt(quantity.toString()),
                null, null);

        Object response = productService.createProduct(product);
        if (response instanceof String)
            redirectAttrs.addFlashAttribute("error", response);
        else
            redirectAttrs.addFlashAttribute("success", "Create product success");

        User user = (User) request.getAttribute("authenticatedUser");
        redirectAttrs.addFlashAttribute("user", user);

        return "redirect:/products";
    }

    @PostMapping("/admin/products/update/{id}")
    public String updateProduct(@PathVariable String id,
                                @RequestParam String name,
                                @RequestParam Category category,
                                @RequestParam Object quantity,
                                @RequestParam Object importPrice,
                                @RequestParam Object retailPrice,
                                @RequestParam String barcode,
                                @RequestParam String illustrator,
                                RedirectAttributes redirectAttrs,
                                HttpServletRequest request) {
        if (name.isEmpty() || barcode.isEmpty() || illustrator.isEmpty() || importPrice == null || retailPrice == null || quantity == null) {
            redirectAttrs.addFlashAttribute("error", "Please fill in all fields");
            return "redirect:/products";
        }

        try {
            Integer.parseInt(quantity.toString());
        } catch (NumberFormatException e) {
            redirectAttrs.addFlashAttribute("error", "Quantity must be a number");
            return "redirect:/products";
        }

        try {
            Double.parseDouble(importPrice.toString());
            Double.parseDouble(retailPrice.toString());
        } catch (NumberFormatException e) {
            redirectAttrs.addFlashAttribute("error", "Price must be a number");
            return "redirect:/products";
        }

        Product product = new Product(id, name, category,
                Double.parseDouble(importPrice.toString()),
                Double.parseDouble(retailPrice.toString()),
                barcode, illustrator,
                Integer.parseInt(quantity.toString()),
                null, null);

        Object response = productService.updateProduct(product);
        if (response instanceof String)
            redirectAttrs.addFlashAttribute("error", response);
        else
            redirectAttrs.addFlashAttribute("success", "Update product success");

        User user = (User) request.getAttribute("authenticatedUser");
        redirectAttrs.addFlashAttribute("user", user);

        return "redirect:/products";
    }

    @PostMapping("/admin/products/delete/{id}")
    public String deleteProduct(@PathVariable String id, RedirectAttributes redirectAttrs) {
        Object response = productService.deleteProduct(id);

        if (response instanceof String)
            redirectAttrs.addFlashAttribute("error", response);
        else
            redirectAttrs.addFlashAttribute("success", "Delete product success");

        return "redirect:/products";
    }

}
