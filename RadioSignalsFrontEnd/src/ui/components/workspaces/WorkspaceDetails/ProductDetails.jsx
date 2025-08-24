import React from 'react';
import {useNavigate, useParams} from "react-router";
import useProductDetails from "../../../../hooks/useProductDetails.js";
import {
    Box,
    Button,
    Card,
    CardContent,
    Chip,
    CircularProgress,
    Divider,
    Grid,
    Typography,
    Paper,
    Avatar,
    Stack,
    Rating,
    Breadcrumbs,
    Link
} from "@mui/material";
import {
    ArrowBack,
    Category,
    Factory,
    Star,
    ShoppingCart,
    FavoriteBorder,
    Share
} from "@mui/icons-material";
import shoppingCartRepository from "../../../../repository/shoppingCartRepository.js";

const ProductDetails = () => {
    const {id} = useParams();
    const {product, category, manufacturer} = useProductDetails(id);
    const navigate = useNavigate();

    if (!product || !category || !manufacturer) {
        return (
            <Box sx={{display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh'}}>
                <CircularProgress/>
            </Box>
        );
    }

    const addToCart = () => {
        shoppingCartRepository
            .addToCart(id)
            .then(() => console.log(`Successfully added product with ID ${id} to card.`))
            .catch((error) => console.log(error));
    };

    return (
        <Box>
            <Breadcrumbs aria-label="breadcrumb" sx={{mb: 3}}>
                <Link
                    underline="hover"
                    color="inherit"
                    href="#"
                    onClick={(e) => {
                        e.preventDefault();
                        navigate("/products");
                    }}
                >
                    Products
                </Link>
                <Typography color="text.primary">{product.name}</Typography>
            </Breadcrumbs>

            <Paper elevation={2} sx={{p: 4, borderRadius: 4}}>
                <Grid container spacing={4}>
                    <Grid size={{xs: 12, md: 3}}>
                        <Box sx={{
                            display: 'flex',
                            justifyContent: 'center',
                            mb: 4,
                            bgcolor: 'background.paper',
                            p: 3,
                            borderRadius: 3,
                            boxShadow: 1
                        }}>
                            <Avatar
                                src={product.image || "/placeholder-product.jpg"}
                                variant="rounded"
                                sx={{
                                    width: '100%',
                                    height: 'auto',
                                    objectFit: 'contain'
                                }}
                            />
                        </Box>
                    </Grid>
                    <Grid size={{xs: 12, md: 9}}>
                        <Box sx={{mb: 3}}>
                            <Typography variant="h3" gutterBottom sx={{fontWeight: 600}}>
                                {product.name}
                            </Typography>

                            <Stack direction="row" spacing={1} alignItems="center" sx={{mb: 2}}>
                                <Rating
                                    value={product.rating || 4.5}
                                    precision={0.5}
                                    readOnly
                                    emptyIcon={<Star style={{opacity: 0.55}} fontSize="inherit"/>}
                                />
                                <Typography variant="body2" color="text.secondary">
                                    ({product.reviewCount || '12'} reviews)
                                </Typography>
                            </Stack>

                            <Typography variant="h4" color="primary.main" sx={{mb: 3}}>
                                ${product.price}
                            </Typography>

                            <Typography variant="body1" sx={{mb: 3}}>
                                Lorem ipsum dolor sit amet, consectetur adipisicing elit. Animi consequatur culpa
                                doloribus, enim maiores possimus similique totam ut veniam voluptatibus. Adipisci
                                consequatur, cum dolorem eaque enim explicabo fugit harum, id laborum non totam ut!
                                Architecto assumenda deserunt doloribus ducimus labore magnam officiis sunt. Autem culpa
                                eaque obcaecati quasi, quo vitae.
                            </Typography>

                            <Stack direction="row" spacing={1} sx={{mb: 3}}>
                                <Chip
                                    icon={<Category/>}
                                    label={category.name}
                                    color="primary"
                                    variant="outlined"
                                    sx={{p: 2}}
                                />
                                <Chip
                                    icon={<Factory/>}
                                    label={manufacturer.name}
                                    color="secondary"
                                    variant="outlined"
                                    sx={{p: 2}}
                                />
                            </Stack>
                        </Box>
                    </Grid>
                    <Grid size={12} display="flex" justifyContent="space-between">
                        <Stack direction="row" spacing={2}>
                            <Button
                                variant="contained"
                                color="primary"
                                startIcon={<ShoppingCart/>}
                                size="large"
                                onClick={addToCart}
                            >
                                Add to Cart
                            </Button>
                            <Button
                                variant="outlined"
                                color="secondary"
                                startIcon={<FavoriteBorder/>}
                            >
                                Wishlist
                            </Button>
                            <Button
                                variant="outlined"
                                startIcon={<Share/>}
                            >
                                Share
                            </Button>
                        </Stack>
                        <Button
                            variant="outlined"
                            startIcon={<ArrowBack/>}
                            onClick={() => navigate("/products")}
                        >
                            Back to Products
                        </Button>
                    </Grid>
                </Grid>
            </Paper>
        </Box>
    );
};

export default ProductDetails;